
// VARIABLES GLOBALES
let currentStep = 1;
let totalSteps = 2;
let selectedRoles = [];
let stepFlow = [];

// CONFIGURACIÓN DE PASOS SEGÚN ROLES
const stepConfig = {
    base: [
        { id: 1, title: "Información Personal", icon: "fas fa-user" },
        { id: 2, title: "Direcciones", icon: "fas fa-map-marker-alt" }
    ],
    prestador: [
        { id: 3, title: "Información Profesional", icon: "fas fa-briefcase" },
        { id: 4, title: "Servicios", icon: "fas fa-tasks" },
        { id: 5, title: "Disponibilidad", icon: "fas fa-calendar-alt" }
    ],
    dueno: [
        { id: 6, title: "Mascotas", icon: "fas fa-paw" }
    ]
};

// INICIALIZACIÓN
document.addEventListener('DOMContentLoaded', function () {
    agregarDireccion();
    generateSidebarSteps();

    // ✅ NUEVO: Event listeners para servicios ofrecidos
    const checkboxesServicios = document.querySelectorAll('input[name="serviciosOfrecidos"]');
    if (checkboxesServicios.length > 0) {
        checkboxesServicios.forEach(checkbox => {
            checkbox.addEventListener('change', function () {
                const errorElement = document.getElementById('error-serviciosOfrecidos');
                if (errorElement && document.querySelectorAll('input[name="serviciosOfrecidos"]:checked').length > 0) {
                    errorElement.style.display = 'none';
                }
            });
        });
    }
});

// GESTIÓN DE ROLES
function toggleRole(card, role) {
    card.classList.toggle('selected');
    const checkbox = card.querySelector('input[type="checkbox"]');
    checkbox.checked = card.classList.contains('selected');
    
    if (checkbox.checked) {
        if (!selectedRoles.includes(role)) {
            selectedRoles.push(role);
        }
    } else {
        selectedRoles = selectedRoles.filter(r => r !== role);
    }
    
    updateWizardFlow();
    clearRoleError();
}

function updateWizardFlow() {
    stepFlow = [...stepConfig.base];
    totalSteps = 2;

    if (selectedRoles.includes('prestador')) {
        stepFlow = [...stepFlow, ...stepConfig.prestador];
        totalSteps += 3;
    }

    if (selectedRoles.includes('dueno')) {
        stepFlow = [...stepFlow, ...stepConfig.dueno];
        totalSteps += 1;
    }

    generateSidebarSteps();
    updateProgress();
}

function generateSidebarSteps() {
    const sidebarProgress = document.getElementById('sidebar-progress');
    sidebarProgress.innerHTML = '';

    stepFlow.forEach((step, index) => {
        const stepElement = document.createElement('div');
        stepElement.className = 'step visible';
        if (step.id === currentStep) stepElement.classList.add('active');
        if (step.id < currentStep) stepElement.classList.add('completed');

        stepElement.innerHTML = `
            <div class="step-number">${index + 1}</div>
            <div class="step-title">${step.title}</div>
        `;
        sidebarProgress.appendChild(stepElement);
    });
}

// NAVEGACIÓN
function nextStep() {
    if (!validateCurrentStep()) {
        return;
    }

    if (currentStep === 1) {
        if (selectedRoles.length === 0) {
            showRoleError();
            return;
        }
        updateWizardFlow();
    }

    const currentStepIndex = stepFlow.findIndex(step => step.id === currentStep);
    if (currentStepIndex === stepFlow.length - 1) {
        finishRegistration();
        return;
    }

    hideCurrentStep();
    currentStep = stepFlow[currentStepIndex + 1].id;
    showCurrentStep();
    updateNavigation();
    updateProgress();
    generateSidebarSteps();
}

function previousStep() {
    const currentStepIndex = stepFlow.findIndex(step => step.id === currentStep);
    if (currentStepIndex > 0) {
        hideCurrentStep();
        currentStep = stepFlow[currentStepIndex - 1].id;
        showCurrentStep();
        updateNavigation();
        updateProgress();
        generateSidebarSteps();
    }
}

function hideCurrentStep() {
    document.getElementById(`step-${currentStep}`).classList.remove('active');
}

function showCurrentStep() {
    document.getElementById(`step-${currentStep}`).classList.add('active');
    initializeStepContent(currentStep);
}

function updateNavigation() {
    const prevBtn = document.getElementById('prev-btn');
    const nextBtn = document.getElementById('next-btn');
    
    const currentStepIndex = stepFlow.findIndex(step => step.id === currentStep);
    
    prevBtn.disabled = currentStepIndex === 0;
    
    if (currentStepIndex === stepFlow.length - 1) {
        nextBtn.innerHTML = 'Siguiente <i class="fas fa-arrow-right"></i>';
        nextBtn.className = 'btn btn-primary';
    }
}

function updateProgress() {
    const currentStepIndex = stepFlow.findIndex(step => step.id === currentStep);
    const progress = ((currentStepIndex + 1) / stepFlow.length) * 100;
    document.getElementById('progress-bar').style.width = progress + '%';
}

// VALIDACIONES
function validateCurrentStep() {
    const currentStepEl = document.getElementById(`step-${currentStep}`);
    const requiredInputs = currentStepEl.querySelectorAll('input[required], select[required], textarea[required]');
    let isValid = true;

    // Limpiar errores previos
    currentStepEl.querySelectorAll('.form-group').forEach(group => {
        group.classList.remove('error');
        const errorMsg = group.querySelector('.error-message');
        if (errorMsg) errorMsg.textContent = '';
    });

    requiredInputs.forEach(input => {
        if (!input.value.trim()) {
            showInputError(input, 'Este campo es obligatorio');
            isValid = false;
        } else if (input.type === 'email' && !isValidEmail(input.value)) {
            showInputError(input, 'Email no válido');
            isValid = false;
        } else if (input.id === 'confirm-password' && input.value !== document.getElementById('password').value) {
            showInputError(input, 'Las contraseñas no coinciden');
            isValid = false;
        }
    });

    if (currentStep === 1 && selectedRoles.length === 0) {
        showRoleError();
        isValid = false;
    }

    if (currentStep === 2) {
        const direcciones = document.querySelectorAll('#direcciones-container .dynamic-item');
        if (direcciones.length === 0) {
            alert('Debe agregar al menos una dirección');
            isValid = false;
        }
    }
    if (currentStep === 3) {
        // ✅ NUEVO: Validar servicios ofrecidos
        const serviciosOfrecidos = document.querySelectorAll('input[name="serviciosOfrecidos"]:checked');
        if (serviciosOfrecidos.length === 0) {
            const errorElement = document.getElementById('error-serviciosOfrecidos');
            if (errorElement) {
                errorElement.textContent = 'Debe seleccionar al menos un servicio';
                errorElement.style.display = 'block';
            }
            isValid = false;
        }
    }

    if (currentStep === 4) {
        const servicios = document.querySelectorAll('#servicios-container .dynamic-item');
        if (servicios.length === 0) {
            alert('Debe agregar al menos un servicio');
            isValid = false;
        }
    }

    if (currentStep === 5) {
        const disponibilidad = document.querySelectorAll('#disponibilidad-container .dynamic-item');
        if (disponibilidad.length === 0) {
            alert('Debe agregar al menos un horario de disponibilidad');
            isValid = false;
        }
    }

    return isValid;
}

function showInputError(input, message) {
    const formGroup = input.closest('.form-group');
    formGroup.classList.add('error');
    const errorMsg = formGroup.querySelector('.error-message');
    if (errorMsg) errorMsg.textContent = message;
}

function showRoleError() {
    document.getElementById('roles-error').textContent = 'Debe seleccionar al menos un rol';
}

function clearRoleError() {
    document.getElementById('roles-error').textContent = '';
}

function isValidEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

// GESTIÓN DE DIRECCIONES
function agregarDireccion() {
    const container = document.getElementById('direcciones-container');
    const direccionCount = container.children.length + 1;
    const isFirst = direccionCount === 1;
    
    const nuevaDireccion = document.createElement('div');
    nuevaDireccion.className = 'dynamic-item';
    nuevaDireccion.innerHTML = `
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px;">
            <h4>Dirección ${direccionCount}</h4>
            <div>
                <label style="margin-right: 15px;">
                    <input type="checkbox" class="predeterminada-check" ${isFirst ? 'checked' : ''}> Predeterminada
                </label>
                ${!isFirst ? '<button type="button" class="remove-btn" onclick="eliminarDireccion(this)"><i class="fas fa-trash"></i></button>' : ''}
            </div>
        </div>

        <div class="form-row">
            <div class="form-group" style="flex: 2;">
                <label>Dirección Completa *</label>
                <input type="text" class="form-control direccion-input" placeholder="Ej: Calle 123 #45-67" required>
                <div class="error-message"></div>
            </div>
            <div class="form-group">
                <label>Ciudad *</label>
                <input type="text" class="form-control ciudad-input" required>
                <div class="error-message"></div>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>Departamento *</label>
                <select class="form-control departamento-select" required>
                    <option value="">Seleccionar...</option>
                    <option value="Cundinamarca">Cundinamarca</option>
                    <option value="Antioquia">Antioquia</option>
                    <option value="Valle del Cauca">Valle del Cauca</option>
                    <option value="Atlántico">Atlántico</option>
                    <option value="Santander">Santander</option>
                    <option value="Bolívar">Bolívar</option>
                </select>
                <div class="error-message"></div>
            </div>
            <div class="form-group">
                <label>País *</label>
                <select class="form-control pais-select" required>
                    <option value="Colombia" selected>Colombia</option>
                    <option value="Venezuela">Venezuela</option>
                    <option value="Ecuador">Ecuador</option>
                    <option value="Perú">Perú</option>
                </select>
                <div class="error-message"></div>
            </div>
        </div>
    `;
    
    container.appendChild(nuevaDireccion);
}

function eliminarDireccion(button) {
    button.closest('.dynamic-item').remove();
    const direcciones = document.querySelectorAll('#direcciones-container .dynamic-item');
    direcciones.forEach((dir, index) => {
        const h4 = dir.querySelector('h4');
        h4.textContent = `Dirección ${index + 1}`;
    });
}

// GESTIÓN DE SERVICIOS
function agregarServicio() {
    const container = document.getElementById('servicios-container');
    const servicioCount = container.children.length + 1;
    
    const nuevoServicio = document.createElement('div');
    nuevoServicio.className = 'dynamic-item';
    nuevoServicio.innerHTML = `
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px;">
            <h4>Servicio ${servicioCount}</h4>
            <button type="button" class="remove-btn" onclick="eliminarServicio(this)">
                <i class="fas fa-trash"></i>
            </button>
        </div>

        <div class="form-row">
            <div class="form-group" style="flex: 2;">
                <label>Nombre del Servicio *</label>
                <input type="text" class="form-control servicio-nombre-input" placeholder="Ej: Paseo de perros" required>
                <div class="error-message"></div>
            </div>
            <div class="form-group">
                <label>Precio (COP) *</label>
                <input type="number" class="form-control servicio-precio-input" placeholder="15000" min="1000" required>
                <div class="error-message"></div>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>Descripción del Servicio *</label>
                <textarea class="form-control servicio-descripcion-input" placeholder="Describe qué incluye este servicio..." required></textarea>
                <div class="error-message"></div>
            </div>
        </div>
    `;
    
    container.appendChild(nuevoServicio);
}

function eliminarServicio(button) {
    button.closest('.dynamic-item').remove();
    const servicios = document.querySelectorAll('#servicios-container .dynamic-item');
    servicios.forEach((serv, index) => {
        const h4 = serv.querySelector('h4');
        h4.textContent = `Servicio ${index + 1}`;
    });
}

// GESTIÓN DE DISPONIBILIDAD
function agregarDisponibilidad() {
    const container = document.getElementById('disponibilidad-container');
    const dispCount = container.children.length + 1;
    
    const nuevaDisponibilidad = document.createElement('div');
    nuevaDisponibilidad.className = 'dynamic-item';
    nuevaDisponibilidad.innerHTML = `
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px;">
            <h4>Horario ${dispCount}</h4>
            <button type="button" class="remove-btn" onclick="eliminarDisponibilidad(this)">
                <i class="fas fa-trash"></i>
            </button>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>Día de la Semana *</label>
                <select class="form-control dia-semana-select" required>
                    <option value="">Seleccionar...</option>
                    <option value="Lunes">Lunes</option>
                    <option value="Martes">Martes</option>
                    <option value="Miércoles">Miércoles</option>
                    <option value="Jueves">Jueves</option>
                    <option value="Viernes">Viernes</option>
                    <option value="Sábado">Sábado</option>
                    <option value="Domingo">Domingo</option>
                </select>
                <div class="error-message"></div>
            </div>
            <div class="form-group">
                <label>Hora Inicio *</label>
                <input type="time" class="form-control hora-inicio-input" required>
                <div class="error-message"></div>
            </div>
            <div class="form-group">
                <label>Hora Fin *</label>
                <input type="time" class="form-control hora-fin-input" required>
                <div class="error-message"></div>
            </div>
        </div>
    `;
    
    container.appendChild(nuevaDisponibilidad);
}

function eliminarDisponibilidad(button) {
    button.closest('.dynamic-item').remove();
    const disponibilidades = document.querySelectorAll('#disponibilidad-container .dynamic-item');
    disponibilidades.forEach((disp, index) => {
        const h4 = disp.querySelector('h4');
        h4.textContent = `Horario ${index + 1}`;
    });
}

// GESTIÓN DE MASCOTAS
function agregarMascota() {
    const container = document.getElementById('mascotas-container');
    const mascotaCount = container.children.length + 1;
    
    const nuevaMascota = document.createElement('div');
    nuevaMascota.className = 'dynamic-item';
    nuevaMascota.innerHTML = `
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px;">
            <h4>Mascota ${mascotaCount}</h4>
            <button type="button" class="remove-btn" onclick="eliminarMascota(this)">
                <i class="fas fa-trash"></i>
            </button>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>Nombre *</label>
                <input type="text" class="form-control mascota-nombre-input" placeholder="Ej: Max" required>
                <div class="error-message"></div>
            </div>
            <div class="form-group">
                <label>Tipo *</label>
                <select class="form-control mascota-tipo-select" required>
                    <option value="">Seleccionar...</option>
                    <option value="Perro">Perro</option>
                    <option value="Gato">Gato</option>
                    <option value="Ave">Ave</option>
                    <option value="Pez">Pez</option>
                    <option value="Hamster">Hamster</option>
                    <option value="Conejo">Conejo</option>
                    <option value="Otro">Otro</option>
                </select>
                <div class="error-message"></div>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>Raza</label>
                <input type="text" class="form-control mascota-raza-input" placeholder="Ej: Labrador">
            </div>
            <div class="form-group">
                <label>Edad (años)</label>
                <input type="number" class="form-control mascota-edad-input" min="0" max="30" placeholder="5">
            </div>
        </div>
    `;
    
    container.appendChild(nuevaMascota);
}

function eliminarMascota(button) {
    button.closest('.dynamic-item').remove();
    const mascotas = document.querySelectorAll('#mascotas-container .dynamic-item');
    mascotas.forEach((mascota, index) => {
        const h4 = mascota.querySelector('h4');
        h4.textContent = `Mascota ${index + 1}`;
    });
}

// FINALIZAR REGISTRO
// FINALIZAR REGISTRO
function finishRegistration() {
    console.log("=== INICIANDO finishRegistration ===");

    const formData = collectFormData();

    console.log('Datos a enviar:', formData);
    console.log('RolesSeleccionados tipo:', typeof formData.RolesSeleccionados);
    console.log('RolesSeleccionados valor:', formData.RolesSeleccionados);

    const nextBtn = document.getElementById('next-btn');
    nextBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Procesando...';
    nextBtn.disabled = true;

    submitToBackend(formData)
        .then(response => {
            console.log("Respuesta exitosa:", response);
            showSuccessStep();
        })
        .catch(error => {
            console.error('Error capturado:', error);
            console.error('Error mensaje:', error.message);
            console.error('Error stack:', error.stack);
            alert('Error al procesar el registro: ' + error.message);
            nextBtn.innerHTML = '<i class="fas fa-check"></i> Finalizar Registro';
            nextBtn.disabled = false;
        });
}

async function submitToBackend(data) {
    try {
        console.log("=== INICIANDO submitToBackend ===");
        console.log("URL destino: /Registro1/ProcesarRegistro");

        const tokenInput = document.querySelector('[name=__RequestVerificationToken]');
        console.log("Token encontrado:", tokenInput ? "Sí" : "No");

        const payload = {
            ...data,
            __RequestVerificationToken: tokenInput ? tokenInput.value : ''
        };

        console.log("Payload a enviar:", payload);
        console.log("JSON stringificado:", JSON.stringify(payload));

        const response = await fetch('/Registro1/ProcesarRegistro', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        console.log("Response status:", response.status);
        console.log("Response ok:", response.ok);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        console.log("Resultado JSON:", result);

        if (!result.success) {
            console.error("Success es false. Errores:", result.errors);
            throw new Error(result.message || 'Error desconocido');
        }

        return result;
    } catch (error) {
        console.error('Error en submitToBackend:', error);
        throw error;
    }
}


  
// FINALIZAR REGISTRO

function collectFormData() {
    // 1. Obtener los elementos de roles seleccionados (los checkboxes ocultos)
    const selectedRoleInputs = document.querySelectorAll('.role-card input[name="roles[]"]:checked');

    // 2. Extraer el valor. DEBEMOS DEJARLOS COMO STRINGS NUMÉRICOS: ["1", "2"]
    const selectedRolesStrings = Array.from(selectedRoleInputs).map(input => input.value);

    console.log("Roles seleccionados (IDs como strings):", selectedRolesStrings);

    const data = {
        Usuario: document.getElementById('usuario').value,
        Email: document.getElementById('email').value,
        Password: document.getElementById('password').value,
        ConfirmPassword: document.getElementById('confirm-password').value,
        PrimerNombre: document.getElementById('primer-nombre').value,
        SegundoNombre: document.getElementById('segundo-nombre').value || null,
        PrimerApellido: document.getElementById('primer-apellido').value,
        SegundoApellido: document.getElementById('segundo-apellido').value || null,
        Telefono: document.getElementById('telefono').value,
        Documento: document.getElementById('documento').value,
        FechaNacimiento: document.getElementById('fecha-nacimiento').value,
        // ENVIAMOS EL ARRAY DE STRINGS NUMÉRICOS: ["1", "2"]
        RolesSeleccionados: selectedRolesStrings,
        Direcciones: [],
        InfoPrestador: null,
        Mascotas: []
    };

    // Direcciones
    document.querySelectorAll('#direcciones-container .dynamic-item').forEach(dir => {
        data.Direcciones.push({
            Direccion: dir.querySelector('.direccion-input').value,
            Ciudad: dir.querySelector('.ciudad-input').value,
            Departamento: dir.querySelector('.departamento-select').value,
            Pais: dir.querySelector('.pais-select').value,
            EsPredeterminada: dir.querySelector('.predeterminada-check').checked
        });
    });

    // Prestador
    // LA CONDICIÓN AHORA ES CON EL STRING "2"
    if (selectedRolesStrings.includes("2")) {
        // ✅ NUEVO: Obtener servicios ofrecidos seleccionados
        const checkboxesServicios = document.querySelectorAll('input[name="serviciosOfrecidos"]:checked');
        const serviciosOfrecidosArray = Array.from(checkboxesServicios).map(cb => cb.value);

        data.InfoPrestador = {
            Resumen: document.getElementById('resumen').value,
            Habilidades: document.getElementById('habilidades').value,
            ServiciosOfrecidos: serviciosOfrecidosArray, // ✅ NUEVO
            Experiencia: document.getElementById('experiencia').value || null,
            AnosExperiencia: parseInt(document.getElementById('anos-experiencia').value) || 0,
            Servicios: [],
            Disponibilidad: []
        };

        document.querySelectorAll('#servicios-container .dynamic-item').forEach(srv => {
            data.InfoPrestador.Servicios.push({
                Nombre: srv.querySelector('.servicio-nombre-input').value,
                Descripcion: srv.querySelector('.servicio-descripcion-input').value,
                Precio: parseFloat(srv.querySelector('.servicio-precio-input').value) || 0 // Usar parseFloat para Precio
            });
        });

        document.querySelectorAll('#disponibilidad-container .dynamic-item').forEach(disp => {
            data.InfoPrestador.Disponibilidad.push({
                DiaSemana: disp.querySelector('.dia-semana-select').value,
                HoraInicio: disp.querySelector('.hora-inicio-input').value,
                HoraFin: disp.querySelector('.hora-fin-input').value
            });
        });
    }

    // Dueño
    // LA CONDICIÓN AHORA ES CON EL STRING "1"
    if (selectedRolesStrings.includes("1")) {
        document.querySelectorAll('#mascotas-container .dynamic-item').forEach(mascota => {
            const nombre = mascota.querySelector('.mascota-nombre-input').value;
            if (nombre.trim()) {
                data.Mascotas.push({
                    Nombre: nombre,
                    Tipo: mascota.querySelector('.mascota-tipo-select').value,
                    Raza: mascota.querySelector('.mascota-raza-input').value || null,
                    Edad: mascota.querySelector('.mascota-edad-input').value ?
                        parseInt(mascota.querySelector('.mascota-edad-input').value) : null
                });
            }
        });
    }

    return data;
}
// BACKEND CONNECTION


function showSuccessStep() {
    hideCurrentStep();
    document.getElementById('step-final').classList.add('active');
    document.getElementById('navigation-buttons').style.display = 'none';
    generateFinalSidebar();
}

function generateFinalSidebar() {
    const sidebarProgress = document.getElementById('sidebar-progress');
    sidebarProgress.innerHTML = '';

    stepFlow.forEach((step, index) => {
        const stepElement = document.createElement('div');
        stepElement.className = 'step visible completed';

        stepElement.innerHTML = `
            <div class="step-number"><i class="fas fa-check"></i></div>
            <div class="step-title">${step.title}</div>
        `;
        sidebarProgress.appendChild(stepElement);
    });
}

function initializeStepContent(stepId) {
    switch(stepId) {
        case 4: // Servicios
            if (document.querySelectorAll('#servicios-container .dynamic-item').length === 0) {
                agregarServicio();
            }
            break;
        case 5: // Disponibilidad
            if (document.querySelectorAll('#disponibilidad-container .dynamic-item').length === 0) {
                agregarDisponibilidad();
            }
            break;
    }
}

// VALIDACIÓN EN TIEMPO REAL
document.addEventListener('input', function(e) {
    if (e.target.hasAttribute('required') || e.target.type === 'email') {
        const formGroup = e.target.closest('.form-group');
        if (formGroup) {
            formGroup.classList.remove('error');
            const errorMsg = formGroup.querySelector('.error-message');
            if (errorMsg) errorMsg.textContent = '';
        }
    }
});

