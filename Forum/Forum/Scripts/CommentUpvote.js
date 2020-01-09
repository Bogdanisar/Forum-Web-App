
function updateVotes(elem, result) {
    if (result == "") {
        return;
    }

    var tmp = JSON.parse(result);

    elem.querySelector(".vote_count").innerHTML = tmp.VoteCount;
    elem.style.color = (tmp.Voted == "1") ? "blue" : "black";
    elem.style.visibility = "visible";
}

function InitComment(elem, CommentId) {
    $.ajax({
        type: 'GET',
        url: '/UpvoteComment/Initialize',
        data: { 'CommentId': CommentId },
        success: function (result) {
            updateVotes(elem, result);
        }
    });
}

$(document).ready(function () {
    for (elem of document.getElementsByClassName("comment-vote-container")) {
        let CommentId = parseInt(elem.getAttribute("CommentId"), 10);
        InitComment(elem, CommentId);
    }
});

function VoteComment(elem, CommentId) {
    $.ajax({
        type: 'GET',
        url: '/UpvoteComment/Vote',
        data: { 'CommentId': CommentId },
        success: function (result) {
            updateVotes(elem, result);
        },
        error: function (result) {
            alert("Error " + result);
        }
    });
}

function ClearComment(elem, CommentId) {
    $.ajax({
        type: 'GET',
        url: '/UpvoteComment/Clear',
        data: { 'CommentId': CommentId },
        success: function (result) {
            let nelem = $(elem).closest(".form-and-dropdown-wrapper").find(".comment-vote-container").get(0) ;
            updateVotes(nelem, result);
        }
    });
}

