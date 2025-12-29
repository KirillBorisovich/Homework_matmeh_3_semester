function addTestsResults(data) {
    const container = document.getElementById('testResults');
    container.innerHTML = '';

    if (!Array.isArray(data) || data.length === 0) {
        container.innerHTML = '<span class="text-muted">No test results</span>';
        return;
    }

    const sortedAssemblies = [...data].sort((a, b) => {
        const nameA = (a.assemblyName || '').toLowerCase();
        const nameB = (b.assemblyName || '').toLowerCase();
        return nameA.localeCompare(nameB);
    });

    for (const asm of sortedAssemblies) {
        const blocks = asm.output || asm.results || asm.value;
        if (!blocks || blocks.length === 0) {
            continue;
        }

        const card = document.createElement('div');
        card.className = 'card mb-3';

        const header = document.createElement('div');
        header.className = 'card-header fw-bold';
        header.textContent = asm.assemblyName;
        card.appendChild(header);

        const body = document.createElement('div');
        body.className = 'card-body p-0';

        const list = document.createElement('ul');
        list.className = 'list-group list-group-flush';

        fillListWithBlocks(blocks, list);

        body.appendChild(list);
        card.appendChild(body);
        container.appendChild(card);
    }
}

function fillListWithBlocks(blocks, list) {
    const failedOrError = [];
    const passed = [];
    const ignored = [];
    const other = [];

    for (const block of blocks) {
        if (!block || !block.trim()) continue;

        const text = block.trimStart();

        if (text.startsWith('Test Failed:') ||
            text.startsWith('An exception is raised in:')) {
            failedOrError.push(text);
        } else if (text.startsWith('Test Passed:')) {
            passed.push(text);
        } else if (text.startsWith('Test Ignored:')) {
            ignored.push(text);
        } else {
            other.push(text);
        }
    }

    const ordered = [
        ...failedOrError,
        ...passed,
        ...ignored,
        ...other
    ];

    for (const text of ordered) {
        const li = document.createElement('li');
        li.className = 'list-group-item';

        if (text.startsWith('Test Passed:')) {
            li.classList.add('list-group-item-success');
        } else if (text.startsWith('Test Failed:') ||
            text.startsWith('An exception is raised in:')) {
            li.classList.add('list-group-item-danger');
        } else if (text.startsWith('Test Ignored:')) {
            li.classList.add('list-group-item-warning');
        }

        const pre = document.createElement('pre');
        pre.className = 'mb-0';
        pre.textContent = text;

        li.appendChild(pre);
        list.appendChild(li);
    }
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
