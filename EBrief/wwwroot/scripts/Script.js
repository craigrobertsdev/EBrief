let currentDialog = null;
let dialogReference = null;

window.onclick = function (event) {
    if (!event.target.matches('.dropdown-btn')) {
        let dropdowns = document.getElementsByClassName("dropdown-content");
    }
}

function openDialog(element, objRef) {
    currentDialog = element;
    dialogReference = objRef;
    element.addEventListener('close', closeEvent)
    document.addEventListener('keydown', onEscapePressed);
    element.showModal();
}

function closeDialog(element) {
    element.removeEventListener("close", closeEvent);
    element.close();
}

const onEscapePressed = (e, element) => {
    if (e.key !== "Escape") {
        return;
    }

    if (currentDialog.id === "new-court-list") { 
        if (dialogReference !== null) {
            dialogReference.invokeMethodAsync("CloseLoadNewCourtListDialog");
        }
    }

    dialogReference = null;
    document.removeEventListener('keydown', onEscapePressed);
    currentDialog.close();
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

function dropdownIsOpen() {
    const dropdown = document.getElementById("dropdown");
    return dropdown.classList.contains("block");
}

function scrollToBottomOfCourtSitting(id) {
    const container = document.getElementById("defendant-container");
    const courtSitting = document.getElementById(id);

    // when this is figured out, check to see whether the increase in container size owing to the expansion of the courtSitting has already occurred.

    // get the starting offset of the courtSitting
    // scrolltop should be at most the height of the courtSitting + the courtSitting offset
    // if height is greater than the viewport of the container, set the scrolltop to the offset of the courtSitting
    // if setting the scrolltop to the offset of the courtSitting would cause the container to scroll beyond the height of the courtSitting, 
    // set the scrolltop to the height of the offsetHeight - view port height
    let courtSittingOffset = courtSitting.offsetTop;
    let viewPortHeight = container.clientHeight;
}