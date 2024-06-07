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
    if (!courtLists) {
        return null;
    }

    for (const list of courtLists) {
        const obj = JSON.parse(list);
        lists.push(obj);
    }

    return JSON.stringify(lists);
}

function getCourtList(key) {
    const courtLists = JSON.parse(localStorage.getItem("courtLists"));
    if (!courtLists) {
        return null;
    }

    for (const list of courtLists) {
        if (list == key) {
            return list;
        }
    }
}

function removeCourtList(key) {
    localStorage.removeItem(key);
    const courtLists = JSON.parse(localStorage.getItem("courtLists"));
    const filteredCourtLists = courtLists.filter(list => list !== key);
    localStorage.setItem("courtLists", JSON.stringify(filteredCourtLists));
}