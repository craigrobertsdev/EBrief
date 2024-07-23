function openDialog(element) {
    element.addEventListener('close', closeEvent)
    element.showModal();
}

function closeDialog(element) {
    element.removeEventListener("close", closeEvent);
    element.close();
}

function closeEvent() {
    document.activeElement.blur();
}

function setTooltipPosition(id) {
    const caseFileCard = document.getElementById(id);
    const container = document.getElementById("casefile-container");

    if (caseFileCard.offsetWidth + caseFileCard.offsetLeft > container.offsetWidth + container.scrollLeft) {
        // the tooltip is beyond the edge of the container to the right
        return container.offsetWidth - caseFileCard.offsetWidth
    } else if (caseFileCard.offsetLeft - container.scrollLeft < 0) {
        // the tooltip is beyond the edge of the container to the left
        return 0;
    } else {
        return caseFileCard.offsetLeft - container.scrollLeft;
    }
}
