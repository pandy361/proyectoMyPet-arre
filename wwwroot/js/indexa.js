function confirmarEliminacion(idMascota, nombreMascota) {
    if (confirm('¿Estás seguro de que deseas eliminar a ' + nombreMascota + '?')) {
        const formData = new FormData();
        formData.append('id', idMascota);

        fetch('@Url.Action("Eliminar", "Mascotas")', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert(data.message);
                    window.location.reload();
                } else {
                    alert('Error: ' + data.message);
                }
            })
            .catch(error => {
                alert('Error al eliminar la mascota');
                console.error(error);
            });
    }
}