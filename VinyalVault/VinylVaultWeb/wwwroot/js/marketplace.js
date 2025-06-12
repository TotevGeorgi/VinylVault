document.addEventListener("DOMContentLoaded", function () {
    const cards = document.querySelectorAll('.vinyl-card');

    cards.forEach(card => {
        card.addEventListener('click', () => {
            const link = card.querySelector('a');
            if (link) window.location.href = link.href;
        });
    });
});
