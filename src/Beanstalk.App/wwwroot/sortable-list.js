export async function initializeSortable(id, handle, filter, group, pull, put, sort, forceFallback, dotNetRef) {
    const element = document.getElementById(id);
    if (!element) {
        console.error(`Element with id "${id}" not found`);
        return null;
    }

    // Check if Sortable is available
    if (typeof Sortable === 'undefined') {
        console.error('SortableJS is not loaded. Please add the script tag: <script src="https://cdnjs.cloudflare.com/ajax/libs/Sortable/1.13.0/Sortable.min.js"></script>');
        return null;
    }

    const options = {
        animation: 150,
        handle: handle || undefined,
        filter: filter || undefined,
        group: group || undefined,
        pull: pull || undefined,
        put: put,
        sort: sort,
        forceFallback: forceFallback,
        onEnd: function (evt) {
            const oldIndex = evt.oldIndex;
            const newIndex = evt.newIndex;
            
            if (oldIndex !== newIndex && oldIndex !== null && newIndex !== null) {
                // Use setTimeout to ensure DOM has settled
                setTimeout(() => {
                    dotNetRef.invokeMethodAsync('OnSortableUpdate', oldIndex, newIndex);
                }, 0);
            }
        },
        onRemove: function (evt) {
            const oldIndex = evt.oldIndex;
            const newIndex = evt.newIndex;
            dotNetRef.invokeMethodAsync('OnSortableRemove', oldIndex, newIndex);
        }
    };

    // Remove undefined options
    Object.keys(options).forEach(key => {
        if (options[key] === undefined) {
            delete options[key];
        }
    });

    const sortable = Sortable.create(element, options);
    
    return {
        destroy: function() {
            if (sortable) {
                sortable.destroy();
            }
        }
    };
}

