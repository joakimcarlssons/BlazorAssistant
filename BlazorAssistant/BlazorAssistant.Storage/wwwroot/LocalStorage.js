/**
 * Sets an item in local storage
 * @param {any} key The local storage key.
 * @param {any} value The local storage value.
*/
window.setInLocalStorage = (key, value) => {
    localStorage.setItem(key, value);
}

/**
 * Gets an item from local storage.
 * @param {any} key The key of the item to get.
 * @returns The value belonging to the key as a string.
 */
window.getFromLocalStorage = (key) => {
    return localStorage.getItem(key);
}

/**
 * Removes an item from local storage.
 * @param {any} key The key of the item to remove.
 */
window.removeFromLocalStorage = (key) => {
    localStorage.removeItem(key);
}

/** Clears the local storage */
window.clearLocalStorage = () => {
    localStorage.clear();
}