// Chart rendering for the statistics page — Terminal Authority theme.
window.taCharts = {
    _charts: [],

    destroy() {
        this._charts.forEach(c => c.destroy());
        this._charts = [];
    },

    render(d) {
        this.destroy();

        Chart.defaults.color = '#c4c5d9';
        Chart.defaults.font.family = "'Inter', 'Helvetica Neue', sans-serif";
        Chart.defaults.font.size = 11;

        const GRID = 'rgba(67, 70, 86, .35)';
        const PALETTE = ['#adc6ff', '#4edea3', '#ffb95f', '#ff7a6e', '#9d7bff',
                         '#4ed4de', '#ff7ad9', '#ffe05f', '#5b9bff', '#00a572',
                         '#ff9d3c', '#8e90a2', '#7bffea'];

        const make = (id, cfg, hasData) => {
            const el = document.getElementById(id);
            if (!el) return;
            const wrap = el.parentElement;
            let empty = wrap.querySelector('.ta-chart-empty');
            if (!hasData) {
                el.style.display = 'none';
                if (!empty) {
                    empty = document.createElement('div');
                    empty.className = 'ta-chart-empty';
                    empty.textContent = 'Sin datos suficientes — completa los campos correspondientes en la grilla.';
                    wrap.appendChild(empty);
                }
                return;
            }
            el.style.display = '';
            if (empty) empty.remove();
            this._charts.push(new Chart(el, cfg));
        };

        const dict = (o) => ({ labels: Object.keys(o || {}), values: Object.values(o || {}) });
        const soloSinDatos = (o) => {
            const k = Object.keys(o || {});
            return k.length === 0 || (k.length === 1 && k[0].startsWith('SIN '));
        };

        // Pie — by sex
        const sx = dict(d.porSexo);
        make('chSexo', {
            type: 'pie',
            data: {
                labels: sx.labels,
                datasets: [{ data: sx.values, backgroundColor: ['#adc6ff', '#ff7ad9', '#8e90a2'], borderColor: '#0b1326', borderWidth: 2 }]
            },
            options: { plugins: { legend: { position: 'bottom' } } }
        }, !soloSinDatos(d.porSexo));

        // Doughnut — by otorgamiento
        const ot = dict(d.porOtorgamiento);
        const otColors = ot.labels.map(l =>
            l === 'OTORGADO' ? '#4edea3' :
            l === 'DENEGADO' ? '#ff7a6e' :
            l === 'ESPERA EXÁMEN' ? '#ffb95f' :
            l === 'S/SGL' ? '#8e90a2' : '#adc6ff');
        make('chOtorgamiento', {
            type: 'doughnut',
            data: {
                labels: ot.labels,
                datasets: [{ data: ot.values, backgroundColor: otColors, borderColor: '#0b1326', borderWidth: 2 }]
            },
            options: { plugins: { legend: { position: 'bottom' } } }
        }, !soloSinDatos(d.porOtorgamiento));

        // Bar — by license type
        const tl = dict(d.porTipoLicencia);
        make('chTipoLicencia', {
            type: 'bar',
            data: {
                labels: tl.labels,
                datasets: [{ data: tl.values, backgroundColor: '#adc6ff', borderRadius: 2 }]
            },
            options: {
                plugins: { legend: { display: false } },
                scales: {
                    x: { grid: { color: GRID } },
                    y: { grid: { color: GRID }, ticks: { precision: 0 } }
                }
            }
        }, !soloSinDatos(d.porTipoLicencia));

        // Horizontal bar — by estado de la carpeta
        const ec = dict(d.porEstadoCarpeta);
        make('chEstadoCarpeta', {
            type: 'bar',
            data: {
                labels: ec.labels,
                datasets: [{ data: ec.values, backgroundColor: PALETTE, borderRadius: 2 }]
            },
            options: {
                indexAxis: 'y',
                plugins: { legend: { display: false } },
                scales: {
                    x: { grid: { color: GRID }, ticks: { precision: 0 } },
                    y: { grid: { display: false } }
                }
            }
        }, !soloSinDatos(d.porEstadoCarpeta));

        // Histogram — ages
        const edades = d.edades || [];
        const bins = [[0, 17], [18, 25], [26, 35], [36, 45], [46, 55], [56, 65], [66, 120]];
        const activos = bins
            .map(([a, b]) => ({ label: b >= 120 ? `${a}+` : `${a}–${b}`, count: edades.filter(e => e >= a && e <= b).length }))
            .filter((bin, i) => bin.count > 0 || (i > 0 && i < bins.length - 1));
        make('chEdades', {
            type: 'bar',
            data: {
                labels: activos.map(b => b.label),
                datasets: [{ data: activos.map(b => b.count), backgroundColor: '#4edea3', borderRadius: 2, barPercentage: 1, categoryPercentage: .95 }]
            },
            options: {
                plugins: { legend: { display: false } },
                scales: {
                    x: { grid: { display: false }, title: { display: true, text: 'Edad (años)' } },
                    y: { grid: { color: GRID }, ticks: { precision: 0 } }
                }
            }
        }, edades.length > 0);

        // Scatter — age vs processing days
        const disp = (d.dispersion || []).map(p => ({ x: p.edad, y: p.diasTramitacion }));
        make('chDispersion', {
            type: 'scatter',
            data: { datasets: [{ data: disp, backgroundColor: '#ffb95f', pointRadius: 5, pointHoverRadius: 7 }] },
            options: {
                plugins: { legend: { display: false } },
                scales: {
                    x: { title: { display: true, text: 'Edad (años)' }, grid: { color: GRID } },
                    y: { title: { display: true, text: 'Días de tramitación (citación → entrega)' }, grid: { color: GRID }, ticks: { precision: 0 } }
                }
            }
        }, disp.length > 0);
    }
};
