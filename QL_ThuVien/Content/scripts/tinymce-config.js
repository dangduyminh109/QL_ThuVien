document.addEventListener("DOMContentLoaded", function () {
    tinymce.init({
        selector: ".textarea-mce",
        plugins: "lists link image code",
        toolbar:
            "undo redo | bold italic | alignleft aligncenter alignright | bullist numlist | link image | code",
        height: 300,
    });
});
