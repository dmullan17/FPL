// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    //script for navbar mobile
    //$('.ui.vertical.menu').toggle();
    $('.left.menu.open').on("click", function (e) {
        e.preventDefault();
        $('.ui.vertical.menu').toggle();
    });
});

EventTarget.prototype['addEventListenerBase'] = EventTarget.prototype.addEventListener;
EventTarget.prototype.addEventListener = function (type, listener) {
    if (this !== document.querySelector('body') || type !== 'touchmove') {
        this.addEventListenerBase(type, listener);
    }
};

function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}
