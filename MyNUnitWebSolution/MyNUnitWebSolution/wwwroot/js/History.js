async function loadAssemblyHistory(assemblyName) {
    const response = await fetch(`?handler=AssemblyHistory&assemblyName=${encodeURIComponent(assemblyName)}`);
    let data;
    try {
        data = await response.json();
    } catch {
        document.getElementById('assemblyHistory').innerHTML =
            '<div class="alert alert-danger">Ошибка при загрузке истории</div>';
        return;
    }

    renderAssemblyHistory(data, assemblyName);
}

function renderAssemblyHistory(runs, assemblyName) {
    const container = document.getElementById('assemblyHistory');
    container.innerHTML = '';

    if (!runs || runs.length === 0) {
        container.innerHTML = `<span class="text-muted">Нет истории для сборки ${assemblyName}</span>`;
        return;
    }

    let html = `<div class="mb-2"><strong>Сборка:</strong> ${assemblyName}</div>`;

    for (const run of runs) {
        html += `<div class="card mb-3">
                        <div class="card-header">
                            Запуск: ${new Date(run.runStartedAt).toLocaleString()}
                        </div>
                        <div class="card-body p-2">
                            ${formatRunLines(run.lines)}
                        </div>
                     </div>`;
    }

    container.innerHTML = html;
}

function formatRunLines(lines) {
    if (!lines || lines.length === 0) {
        return '<span class="text-muted">Нет строк вывода</span>';
    }

    const failedOrError = [];
    const passed = [];
    const ignored = [];
    const other = [];

    for (const block of lines) {
        if (!block || !block.trim()) continue;
        const text = block.trimStart();

        if (text.includes('Test Failed:') || text.includes('An exception is raised in:')) {
            failedOrError.push(text);
        } else if (text.includes('Test Passed:')) {
            passed.push(text);
        } else if (text.includes('Test Ignored:')) {
            ignored.push(text);
        } else {
            other.push(text);
        }
    }

    const ordered = [...failedOrError, ...passed, ...ignored, ...other];

    let html = '<ul class="list-group list-group-flush">';
    for (const text of ordered) {
        let rowClass = 'list-group-item';
        if (text.includes('Test Passed:')) {
            rowClass += ' list-group-item-success';
        } else if (text.includes('Test Failed:') || text.includes('An exception is raised in:')) {
            rowClass += ' list-group-item-danger';
        } else if (text.includes('Test Ignored:')) {
            rowClass += ' list-group-item-warning';
        }

        html += `<li class="${rowClass}"><pre class="mb-0">${escapeHtml(text)}</pre></li>`;
    }
    html += '</ul>';

    return html;
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.innerText = text;
    return div.innerHTML;
}