window.fednadShowroom = {
    scrollProducts: (element, direction) => {
        if (!element) return;
        const card = element.querySelector(".fn-product-card");
        if (!card) return;

        const styles = window.getComputedStyle(element);
        const gap = parseFloat(styles.columnGap || styles.gap || "0");
        const distance = card.getBoundingClientRect().width + gap;

        element.scrollBy({
            left: direction * distance,
            behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "auto" : "smooth"
        });
    }
};