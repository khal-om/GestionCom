window.fednadShowroom = {
    scrollProducts: (element, direction) => {
        if (!element) return;

        const firstCard = element.querySelector(".fd-belt-product");
        const track = element.querySelector(".fd-belt-track");
        if (!firstCard) return;

        const cardWidth = firstCard.getBoundingClientRect().width;
        const gap = track ? parseFloat(getComputedStyle(track).columnGap || "0") : 0;

        // Move exactly one card so visitors can browse forwards and backwards
        // without skipping products.
        element.scrollBy({
            left: direction * (cardWidth + gap),
            behavior: "smooth"
        });
    }
};
