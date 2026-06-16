/**
 * Restringe el selector de años en inputs type="date".
 * Calcula el año máximo (hoy - 17 años) y lo aplica como atributo "max".
 */
(function () {
    var ANIOS_MINIMOS = 17;

    function calcularMaxFecha() {
        var hoy = new Date();
        var maxAnio = hoy.getFullYear() - ANIOS_MINIMOS;
        var mes = String(hoy.getMonth() + 1).padStart(2, '0');
        var dia = String(hoy.getDate()).padStart(2, '0');
        return maxAnio + '-' + mes + '-' + dia;
    }

    function aplicarRestriccion() {
        var maxFecha = calcularMaxFecha();
        document.querySelectorAll('input[type="date"]').forEach(function (input) {
            if (!input.getAttribute('max') || input.getAttribute('max') !== maxFecha) {
                input.setAttribute('max', maxFecha);
            }
        });
    }

    // Ejecutar al cargar
    aplicarRestriccion();

    // Observar el DOM por si se agregan nuevos inputs date
    var observer = new MutationObserver(function () {
        aplicarRestriccion();
    });
    observer.observe(document.body, { childList: true, subtree: true });

    // También al cambiar cualquier input date
    document.addEventListener('change', function (e) {
        if (e.target.type === 'date') {
            aplicarRestriccion();
        }
    });
})();
