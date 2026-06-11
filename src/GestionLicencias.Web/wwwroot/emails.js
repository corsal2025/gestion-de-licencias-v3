// Email domain autocomplete for inputs with class "ta-email".
// Domains persist in localStorage; defaults always present.
window.taEmail = {
    KEY: 'ta-email-domains',
    defaults: ['gmail.com', 'hotmail.com', 'outlook.com', 'yahoo.com', 'icloud.com', 'munivalpo.cl'],

    _extras() {
        try { return JSON.parse(localStorage.getItem(this.KEY) || '[]'); }
        catch { return []; }
    },

    getDomains() {
        return [...new Set([...this.defaults, ...this._extras()])];
    },

    addDomain(d) {
        d = (d || '').trim().toLowerCase().replace(/^@/, '');
        if (!d || !d.includes('.')) return this.getDomains();
        const extras = this._extras();
        if (!extras.includes(d) && !this.defaults.includes(d)) {
            extras.push(d);
            localStorage.setItem(this.KEY, JSON.stringify(extras));
        }
        return this.getDomains();
    },

    removeDomain(d) {
        const extras = this._extras().filter(x => x !== d);
        localStorage.setItem(this.KEY, JSON.stringify(extras));
        return this.getDomains();
    },

    isDefault(d) {
        return this.defaults.includes(d);
    },

    _ensureDatalist() {
        let dl = document.getElementById('ta-email-dl');
        if (!dl) {
            dl = document.createElement('datalist');
            dl.id = 'ta-email-dl';
            document.body.appendChild(dl);
        }
        return dl;
    }
};

document.addEventListener('input', (e) => {
    const t = e.target;
    if (!t.matches || !t.matches('input.ta-email')) return;
    const dl = window.taEmail._ensureDatalist();
    const v = t.value || '';
    const at = v.indexOf('@');
    const prefix = at >= 0 ? v.slice(0, at) : v;
    if (!prefix) { dl.innerHTML = ''; return; }
    dl.innerHTML = window.taEmail.getDomains()
        .map(d => `<option value="${prefix}@${d}"></option>`)
        .join('');
});
