async function uploadFile(event) {
    event.preventDefault();

    const fileInput = document.querySelector('input[type="file"][name="UploadFile"]');
    const file = fileInput.files[0];

    if (!file) {
        showMessage('Please select the file to download.', 'warning');
        return;
    }

    const token = document.getElementById('__RequestVerificationToken').value;

    const formData = new FormData();
    formData.append('UploadFile', file);

    const response = await fetch('?handler=UploadFile', {
        method: 'POST',
        headers: {
            'RequestVerificationToken': token
        },
        body: formData
    });

    let result;
    try {
        result = await response.json();
    } catch {
        showMessage('Error processing the server response', 'danger');
        return;
    }

    if (result.success) {
        showMessage(result.message || 'The file was uploaded successfully', 'success');
        addFileToList(result.fileName);
        fileInput.value = '';
    } else {
        showMessage(result.message || 'Error when uploading a file', 'danger');
    }
}

function addFileToList(fileName) {
    if (!fileName) return;

    const ul = document.getElementById('filesList');

    const li = document.createElement('li');
    li.className = 'list-group-item d-flex justify-content-between align-items-center';

    const span = document.createElement('span');
    span.className = 'text-truncate';
    span.style.maxWidth = '80%';
    span.textContent = fileName;

    const button = document.createElement('button');
    button.type = 'button';
    button.className = 'btn btn-sm btn-danger';
    button.textContent = 'Delete';
    button.onclick = function () {
        deleteFile(fileName, button);
    };

    li.appendChild(span);
    li.appendChild(button);
    ul.appendChild(li);
}

function showMessage(text, type) {
    const container = document.getElementById('messageArea');
    container.innerHTML = '';

    if (!text) return;

    const div = document.createElement('div');
    div.className = 'alert alert-' + (type || 'info');
    div.role = 'alert';
    div.textContent = text;

    container.appendChild(div);
}

async function deleteFile(fileName, button) {
    const token = document.getElementById('__RequestVerificationToken').value;

    const response = await fetch('?handler=DeleteFile', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: new URLSearchParams({ fileName })
    });

    if (response.ok) {
        button.closest('li').remove();
        showMessage(`File "${fileName}" deleted`, 'success');
    } else {
        showMessage('Error when deleting a file', 'danger');
    }
}