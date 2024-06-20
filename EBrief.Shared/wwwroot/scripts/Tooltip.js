function setTooltipPosition(id) {
    const caseFileCard = document.getElementById(id);
    const container = document.getElementById("casefile-container");

    return caseFileCard.offsetWidth + caseFileCard.offsetLeft > container.offsetWidth + container.scrollLeft
        ? container.offsetWidth - caseFileCard.offsetWidth // the tooltip is beyond the edge of the container
        : caseFileCard.offsetLeft - container.scrollLeft;
}