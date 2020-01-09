

function VoteSubject(elem, subjectId, event) {
    $.ajax({
        type: 'GET',
        url: '/UpvoteSubject/Vote',
        data: { 'subjectId': JSON.stringify(subjectId) },
        success: function (result) {
            if (result != "") {
                var tmp = JSON.parse(result);

                elem.querySelector(".vote_count").innerHTML = tmp.VoteCount;
                elem.style.color = (tmp.Voted == "1") ? "blue" : "black";
            }
        },
        error: function (result) {
            alert("Error " + result);
        }
    });
}

function Clear(subjectId) {
    $.ajax({
        type: 'GET',
        url: '/UpvoteSubject/Clear/',
        data: { 'subjectId': subjectId },
        success: function (result) {
            alert(result);
        }
    });
}

function InitSubject(elem, sub_id) {
    $.ajax({
        type: 'GET',
        url: '/UpvoteSubject/Initialize',
        data: { 'subjectId': JSON.stringify(sub_id) },
        success: function (result) {
            var tmp = JSON.parse(result);

            elem.querySelector(".vote_count").innerHTML = tmp.VoteCount;
            elem.style.color = (tmp.Voted == "1") ? "blue" : "black";
            elem.style.visibility = "visible";
        }
    });
}


$(document).ready(function () {
    for (elem of document.getElementsByClassName("subject-vote-container")) {
        let subjectId = parseInt(elem.getAttribute("SubjectId"), 10);
        InitSubject(elem, subjectId);
    }
});








