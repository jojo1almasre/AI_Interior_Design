function initializeBeforeAfterSlider() {
    const slider = document.getElementById('slider');
    const afterImage = document.querySelector('.img-after');

    if (!slider || !afterImage) return;

    slider.addEventListener('input', (e) => {
        const value = e.target.value;
        afterImage.style.width = value + '%';
    });

    slider.addEventListener('touchstart', () => {
        document.body.style.overflow = 'hidden';
    });

    slider.addEventListener('touchend', () => {
        document.body.style.overflow = 'auto';
    });
}

document.addEventListener('DOMContentLoaded', initializeBeforeAfterSlider);
