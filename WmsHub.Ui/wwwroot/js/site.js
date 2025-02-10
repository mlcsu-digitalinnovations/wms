// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.body.className = ((document.body.className) ? document.body.className + ' js-enabled' : 'js-enabled');
window._signalRUrl = $('#SignalR_Endpoint').val();

// Add validation css classes
$(document).ready(function () {
    $('.nhsuk-date-input__input').blur(function () {
        var parent = $(this).parents('.nhsuk-fieldset').parent('.nhsuk-form-group');
        if (parent.find('span.field-validation-error').length !== 0 && parent.not('.nhsuk-form-group--error')) {
            parent.addClass('nhsuk-form-group--error');
        };
        if (parent.find('span.field-validation-error').length === 0) {
            parent.removeClass('nhsuk-form-group--error');
        };
    });
});
