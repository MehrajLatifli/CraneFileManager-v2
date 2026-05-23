// swagger-custom.js
document.addEventListener("DOMContentLoaded", function () {
    setTimeout(function () {
        var link = document.createElement("link");
        link.rel = "icon";
        link.type = "image/png";
        link.href = "/swagger-ui/logo2.png"; // Path to your favicon
        link.sizes = "64x64";
        document.head.appendChild(link);
    }, 100);
});
