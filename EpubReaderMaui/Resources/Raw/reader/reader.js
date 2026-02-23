var book = null;
window.rendition = null;

alert("reader.js loaded");

window.loadTestBook = function () {
    alert("loadTestBook called");

    // Public test EPUB (Moby Dick)
    book = ePub("https://s3.amazonaws.com/moby-dick/OPS/package.opf");

    window.rendition = book.renderTo("viewer", {
        width: "100%",
        height: "100%",
        spread: "none"
    });

    window.rendition.display();

    // Send chapter list to MAUI
    book.loaded.navigation.then(function (nav) {
        var chapters = nav.toc.map(function (item) {
            return {
                Title: item.label,
                Href: item.href
            };
        });

        var json = encodeURIComponent(JSON.stringify(chapters));
        window.location.href = "chapters://" + json;
    });
};