export function registerUnloadEvent(dotNetHelper) {
    window.onbeforeunload = () => {
        dotNetHelper.invokeMethodAsync("DisposeResources");
    };
}

export function unregisterUnloadEvent() {
    window.onbeforeunload = null;
}
