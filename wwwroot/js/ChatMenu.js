let lastClickedChat;

async function getChats() {
    if (lastClickedChat != undefined) {
        let formData = new FormData();
        formData.append('currentchatId', lastClickedChat.id);
        var response = await fetch('/Chat/GetChats', {
            method: 'POST',
            body: formData,
        });
    }
    else {
        var response = await fetch('/Chat/GetChats', {
            method: 'POST',
        });
    }
    if (response.redirected) {
        location.reload();
    }
    let result = await response.text();
    document.getElementById('chats').innerHTML = result;
    //console.log('getChats called');
}

function addEventsOnChatClick() {
    var chats = document.getElementsByClassName('chat');
    //console.log('addEventsOnChatClick called');
    for (let chat of chats) {
        chat.addEventListener('click', async function () {
            chat.setAttribute('style', 'background-color:#b1b1b1');
            if (lastClickedChat != undefined) {
                document.getElementById(lastClickedChat.id).setAttribute('style', '');
                let formData = new FormData();
                formData.append('chatId', lastClickedChat.id);
                fetch('/Chat/WriteLastVisitedTimeForChat', {
                    method: 'POST',
                    body: formData,
                });
            }
            lastClickedChat = chat;

            let formData = new FormData();
            formData.append('chatId', chat.id);
            let response = await fetch('/Chat/GetMessages', {
                method: 'POST',
                body: formData,
            });
            let result = await response.text();
            document.getElementById('messages').innerHTML = result;

            eval(document.getElementById("displayUserMenuInMessages").innerHTML);
            eval(document.getElementById("addToBlackListScript").innerHTML);
            AddsendMessageEvents();
            updateCompanionUserStatus();
        });
    }
}

function lastClickedChatCheck() {

    if (lastClickedChat != undefined) {
        document.getElementById(lastClickedChat.id).setAttribute('style', 'background-color:#b1b1b1');
    }

}

let hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .build();

hubConnection.on("Receive", function (messageText, chatId, fromWhom) {
    //console.log(`message=${messageText}`);
    if (chatId == lastClickedChat.id) {
        let now = new Date().toLocaleString('ru-RU', {
            day: "numeric",
            month: "numeric",
            year: "2-digit",
            hour: "numeric",
            minute: "numeric"
        });

        let messageCover = document.createElement("div");
        let mes = document.createElement("div");
        let triangleWrapper = document.createElement("div");
        let triangle = document.createElement("div");
        let messageTime = document.createElement("div");

        if (fromWhom == 'fromCurrentUser') {
            messageCover.setAttribute('class', 'MymessageCover');
            mes.setAttribute('class', 'messageFromMe');
            triangleWrapper.setAttribute('class', 'triangleWrapperRight');
            triangle.setAttribute('class', 'triangle-right');
            messageTime.setAttribute('class', 'MymessageTime');
        }
        else {
            messageCover.setAttribute('class', 'NotMymessageCover');
            mes.setAttribute('class', 'messageFromNotMe');
            triangleWrapper.setAttribute('class', 'triangleWrapperLeft');
            triangle.setAttribute('class', 'triangle-left');
            messageTime.setAttribute('class', 'NotMymessageTime');
        }

        let messageHeight;
        let messageWidth;
        if (messageText.length <= 40) {
            messageWidth = messageText.length * 10 + 10;
            messageHeight = 25 + 15;
        }
        else {
            messageWidth = 40 * 10 + 5;
        }
        if (messageText.length > 40 && messageText.length <= 80) {
            messageHeight = 45;
        }
        if (messageText.length > 80 && messageText.length <= 120) {
            messageHeight = 65;
        }
        if (messageText.length > 120) {
            messageHeight = 18 * Math.ceil(messageText.length / 40);
        }

        mes.setAttribute('style', `height:${messageHeight}px;width:${messageWidth}px`);
        messageCover.setAttribute('style', `height:${messageHeight}px;`);

        messageTime.appendChild(document.createTextNode(now));
        mes.appendChild(document.createTextNode(messageText));
        triangleWrapper.appendChild(triangle);
        messageCover.appendChild(triangleWrapper);
        messageCover.appendChild(mes);
        messageCover.appendChild(messageTime);

        let firstElem = document.getElementById("messageblock").firstChild;
        document.getElementById("messageblock").insertBefore(messageCover, firstElem);
    }
});
hubConnection.start();

function AddsendMessageEvents() {

    function sendMessage() {
        let messagebox = document.getElementById("message");
        let message = messagebox.value;
        let reciever = document.getElementById("userUniqueName").value;
        hubConnection.invoke("Send", message, reciever, lastClickedChat.id);
        messagebox.value = "";
    }

    if (document.getElementById('cannotSendMessageAnyMore') == null) {

        document.getElementById("sendBtn").addEventListener("click", function () {
            sendMessage();
        });
        document.getElementById("message").addEventListener('keypress', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                sendMessage();
                e.preventDefault();
            }
        });
    }
}

async function setUserStatus(status) {
    let formData = new FormData();
    formData.append('status', status);
    await fetch('/User/SetUserStatus', {
        method: 'PUT',
        body: formData,
    });

}

function updateCompanionUserStatus() {
    setInterval(async () => {
        await internalStausUpdate();
    }, 30000);
    async function internalStausUpdate() {
        let companionUserName = document.getElementById('userUniqueName').value;
        let formData = new FormData();
        formData.append('userName', companionUserName);
        let response = await fetch('/User/UpdateCompanionUserStatus', {
            method: 'PUT',
            body: formData,
        });
        let result = await response.text();
        document.getElementById('userStatus').innerHTML = result;
    }
}

async function writeLastVisitedTimeForChat() {
    if (lastClickedChat != undefined) {
        let formData = new FormData();
        formData.append('chatId', lastClickedChat.id);
        let result = await fetch('/Chat/WriteLastVisitedTimeForChat', {
            method: 'POST',
            body: formData,
        });
        let response = await result.status;
    }
}

window.addEventListener("unload", async function (e) {
    await writeLastVisitedTimeForChat();
    await setUserStatus("offline");
    return;
});

window.addEventListener("beforeunload", async function (e) {
    await writeLastVisitedTimeForChat();
    await setUserStatus("offline");
    return;
});

document.addEventListener('DOMContentLoaded', async () => {
    await getChats();
    addEventsOnChatClick();
    await setUserStatus("online");
});

setInterval(async () => {
    await getChats();
    addEventsOnChatClick();
    lastClickedChatCheck();
}, 5000);