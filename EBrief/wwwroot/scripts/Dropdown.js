let dropdownRef;
function dropdownIsOpen() {
    const dropdown = document.getElementById("dropdown");
    return dropdown.classList.contains("block");
}

const closeDropDownOnClickOut = (event) => {
    if (!event.target.matches('.dropdown-btn')) {
        dropdownRef.invokeMethodAsync("Close");
    }
}

function addDropdownClickHandler(objRef) {
    dropdownRef = objRef;
    window.addEventListener("mousedown", closeDropDownOnClickOut);
}

function removeDropdownClickHandler() {
    objRef = null;
    window.removeEventListener("mousedown", closeDropDownOnClickOut);
}
