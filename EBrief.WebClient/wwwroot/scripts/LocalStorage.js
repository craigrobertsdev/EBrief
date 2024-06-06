function saveCourtList(key, courtList) {
    localStorage.setItem(key, courtList);
    let courtLists = JSON.parse(localStorage.getItem("courtLists"));
    if (courtLists) {
        courtLists.push(key);
    } else {
        courtLists = [key];
    }
    localStorage.setItem("courtLists", JSON.stringify(courtLists));
}

function getPreviousCourtLists() {
    const courtLists = JSON.parse(localStorage.getItem("courtLists"));
    const lists = [];
    for (const list of courtLists) {
        const obj = JSON.parse(list);
        lists.push(obj);
    }

    return JSON.stringify(lists);
}