function addTestsResults(result) {
    const container = document.getElementById('testResults');
    container.innerHTML = '';

    if (!result) {
        container.innerHTML = '<span class="text-muted">No test results</span>';
        return;
    }

    let blocks;
    if (Array.isArray(result)) {
        blocks = result;
    } else if (Array.isArray(result.results)) {
        blocks = result.results;
    } else if (Array.isArray(result.output)) {
        blocks = result.output;
    } else {
        console.warn('Unexpected test result format:', result);
        container.innerHTML = '<span class="text-muted">Unexpected test result format</span>';
        return;
    }

    if (blocks.length === 0) {
        container.innerHTML = '<span class="text-muted">No test results</span>';
        return;
    }

    const failedOrError = [];
    const passed = [];
    const ignored = [];
    const other = [];

    for (const block of blocks) {
        if (!block || !block.trim()) continue;

        const text = block.trimStart();

        if (text.startsWith('Test Failed:') ||
            text.startsWith('An exception is raised in:')) {
            failedOrError.push(block);
        } else if (text.startsWith('Test Passed:')) {
            passed.push(block);
        } else if (text.startsWith('Test Ignored:')) {
            ignored.push(block);
        } else {
            other.push(block);
        }
    }

    const orderedBlocks = [
        ...failedOrError,
        ...passed,
        ...ignored,
        ...other
    ];

    if (orderedBlocks.length === 0) {
        container.innerHTML = '<span class="text-muted">No test results</span>';
        return;
    }

    const list = document.createElement('ul');
    list.className = 'list-group';

    for (const block of orderedBlocks) {
        const li = document.createElement('li');
        li.className = 'list-group-item';

        const text = block.trimStart();

        if (text.startsWith('Test Passed:')) {
            li.classList.add('list-group-item-success');
        } else if (text.startsWith('Test Failed:')) {
            li.classList.add('list-group-item-danger');
        } else if (text.startsWith('Test Ignored:')) {
            li.classList.add('list-group-item-warning');
        } else if (text.startsWith('An exception is raised in:')) {
            li.classList.add('list-group-item-danger');
        }

        const pre = document.createElement('pre');
        pre.className = 'mb-0';
        pre.textContent = text;

        li.appendChild(pre);
        list.appendChild(li);
    }

    container.appendChild(list);
}

async function runTheTests() {
    const token = document.getElementById('__RequestVerificationToken').value;

    const response = await fetch('?handler=RunTheTests', {
        method: 'POST',
        headers: {
            'RequestVerificationToken': token
        }
    });

    let result;
    try {
        result = await response.json();
    } catch {
        showMessage('Error processing the server response', 'danger');
        return;
    }

    if (!response.ok || !result.success) {
        showMessage(result.error ?? 'Unknown error', 'danger');
        document.getElementById('testResults').innerHTML = '';
        return;
    }

    addTestsResults(result.data);
}
