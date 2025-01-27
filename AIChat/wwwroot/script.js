function scrollElementToView(element) {
    try {
        element.scrollIntoView({behavior: "smooth"});
    } catch (error) {
        console.log(error);
    }
}