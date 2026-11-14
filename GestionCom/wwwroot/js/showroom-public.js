window.fednadShowroom = {
    scrollProducts: (element, direction) => {
        if (!element) return;
        const firstCard = element.querySelector(".fd-belt-product");
        const cardWidth = firstCard ? firstCard.getBoundingClientRect().width + 22 : 260;
        element.scrollBy({ left: direction * cardWidth * 2, behavior: "smooth" });
    }
};
