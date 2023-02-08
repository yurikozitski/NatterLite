async function getBannedUsers() {
    let response = await fetch('/Admin/GetBannedUsers', {
        method: 'POST',
        body: new FormData(searchForm)
    });
    let result = await response.text();
    document.getElementById('SearchResult').innerHTML = result;

    let banForms = document.getElementsByClassName('banForm');
    let unblockForms = document.getElementsByClassName('unblockForm');

    if (banForms.length != 0) {
        for (let banForm of banForms) {
            if (banForm.getAttribute('name') == 'True') {
                banForm.lastElementChild.setAttribute('disabled', 'true');
            }
            banForm.addEventListener('submit', async function (e) {
                e.preventDefault();
                let response = await fetch('/Admin/BanUser', {
                    method: 'POST',
                    body: new FormData(banForm)
                });
                if (response.status == 200) {
                    banForm.lastElementChild.setAttribute('disabled', 'true');
                    banForm.nextElementSibling.lastElementChild.removeAttribute('disabled');
                }
            });

        }
    }

    if (unblockForms.length != 0) {
        for (let unblockForm of unblockForms) {
            if (unblockForm.getAttribute('name') == 'False') {
                unblockForm.lastElementChild.setAttribute('disabled', 'true');
            }
            unblockForm.addEventListener('submit', async function (e) {
                e.preventDefault();
                let response = await fetch('/Admin/UnblockUser', {
                    method: 'POST',
                    body: new FormData(unblockForm)
                });
                if (response.status == 200) {
                    unblockForm.parentElement.parentElement.parentElement.remove();
                }
            });

        }
    }
}

let searchForm = document.getElementById('searchForm');
searchForm.addEventListener('submit', async function (e) {
    e.preventDefault();
    await getBannedUsers();
});

document.addEventListener("DOMContentLoaded", async function (e) {
    await getBannedUsers();
});

