// Excel file upload — reads .xlsx as base64 for Blazor Server
window.taReadExcel = function (inputId) {
    return new Promise(function (resolve, reject) {
        var input = document.getElementById(inputId);
        if (!input || !input.files || input.files.length === 0) {
            resolve(null);
            return;
        }
        var file = input.files[0];
        var reader = new FileReader();
        reader.onload = function (e) {
            var base64 = e.target.result.split(',')[1];
            resolve([file.name, base64]);
        };
        reader.onerror = function () {
            reject('Error reading file');
        };
        reader.readAsDataURL(file);
    });
};
