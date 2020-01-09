function displayToast(message) {
    let div = document.createElement("div");
    div.innerHTML = message;
    div.className = "snackbar";
    document.body.appendChild(div);

    setTimeout(function () {
        div.className = "snackbar fadeout";
        setTimeout(function () {
            //div.remove();
        }, 5000);
    }, 4000);
}