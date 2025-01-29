function scrollElementToView(element, behavior = null) {
    try {
        element.scrollIntoView({behavior: behavior ?? 'smooth'});
    } catch (error) {
        console.log(error);
    }
}