document.addEventListener('DOMContentLoaded', () => {
    const links = document.querySelectorAll('.nav-link');

    links.forEach(link => {
        link.addEventListener('touchstart', function () {
            links.forEach(l => l.classList.remove('hovered'));
            this.classList.add('hovered');
        });
    });
});