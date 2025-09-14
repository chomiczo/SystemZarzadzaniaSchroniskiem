// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const toastDivs = document.querySelectorAll('.toast')
const toasts = [...toastDivs].map(t => new bootstrap.Toast(t, {}))
