// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {

});

EventTarget.prototype['addEventListenerBase'] = EventTarget.prototype.addEventListener;
EventTarget.prototype.addEventListener = function (type, listener) {
    if (this !== document.querySelector('body') || type !== 'touchmove') {
        this.addEventListenerBase(type, listener);
    }
};