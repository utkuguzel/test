$(document).ready(function () {
    $(".archive-item").on("mouseenter touchstart", function () {
        console.log("hover");

        $(this).find(".hover-border").stop().animate({
            width: "3px"
        }, 100);
    });

    $(".archive-item").on("mouseleave touchend", function () {
        $(this).find(".hover-border").stop().animate({
            width: "0px"
        }, 75);
    });
});