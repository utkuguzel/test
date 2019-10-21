function init() {
    $(".login, .register").on("click", onBeforeSubmit);
    $("div.requirements").hide();
}

function onBeforeSubmit(e) {
    e.preventDefault();

    $("div.requirements").hide().empty();
    $.post("/verify/form/" + $(e.target).attr("class"), $("form").serialize(), function (response) {
        if (response === 200) {
            $("form").submit();
        }
        else {
            $.each(response, function (_, v) {
                $("div.requirements").slideDown(250).append(v.errorMessage + "<br/>");
            });
        }
    });
}

$(document).ready(init);