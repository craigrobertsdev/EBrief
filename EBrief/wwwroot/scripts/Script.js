let currentDialog = null;
let dialogReference = null;
let courtListPageReference = null;

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

    if (currentDialog.id === "new-court-list-dialog") {
        if (dialogReference !== null) {
            dialogReference.invokeMethodAsync("CloseLoadNewCourtListDialog");
        }
    } else if (currentDialog.id === "search-dialog") {
        if (courtListPageReference !== null) {
            removeKeyBoardSearchNavigation(e);
            const searchField = document.getElementById("search-field");
            searchField.value = "";
            courtListPageReference.invokeMethodAsync("ClearSearchResults");
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
    const casefileCard = document.getElementById(id);
    const container = document.getElementById("casefile-container");

    if (casefileCard.offsetWidth + casefileCard.offsetLeft > container.offsetWidth + container.scrollLeft) {
        // the tooltip is beyond the edge of the container to the right
        return container.offsetWidth - casefileCard.offsetWidth
    } else if (casefileCard.offsetLeft - container.scrollLeft < 0) {
        // the tooltip is beyond the edge of the container to the left
        return 0;
    } else {
        return casefileCard.offsetLeft - container.scrollLeft;
    }
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

const deleteCourtListEntry = async (event, objRef) => {
    if (!(event.key === "Delete")) {
        return;
    }

    await objRef.invokeMethodAsync("OpenConfirmDialog");
}

function addDeleteEventHandler(objRef) {
    window.addEventListener("keydown", (e) => deleteCourtListEntry(e, objRef));
}

function removeDeleteEventHandler() {
    window.removeEventListener("keydown", deleteCourtListEntry);
}

const handleSearch = (event) => {
    if (!(event.key === "f" && event.ctrlKey) || !event.key === "F3") {
        return;
    }

    event.preventDefault();

    window.addEventListener("keydown", handleKeyBoardSearchNavigation);
    courtListPageReference.invokeMethodAsync("OpenSearchDialog");
}

function handleKeyBoardSearchNavigation(event) {
    if (event.key === "ArrowDown") {
        courtListPageReference.invokeMethodAsync("SelectNextSearchResult");
    } else if (event.key === "ArrowUp") {
        courtListPageReference.invokeMethodAsync("SelectPreviousSearchResult");
    } else if (event.key === "Enter") {
        courtListPageReference.invokeMethodAsync("SelectSearchResult");
    }
}

function removeKeyBoardSearchNavigation(e) {
    window.removeEventListener("keydown", handleKeyBoardSearchNavigation);
}

function clearSearchText() {
    const searchField = document.getElementById("search-field");
    searchField.value = "";
}

function addSearchEventHandler(courtListPageRef) {
    courtListPageReference = courtListPageRef;
    window.addEventListener("keydown", handleSearch);
}

function removeSearchEventHandler() {
    window.removeEventListener("keydown", handleSearch);
    window.removeEventListener("keydown", handleKeyBoardSearchNavigation);
}
