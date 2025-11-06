// Image cropper functionality
let cropper = null;

window.handleImageFileSelected = function(input) {
    const file = input.files[0];
    if (!file) return;

    // Validate file size (10MB)
    if (file.size > 10 * 1024 * 1024) {
        alert('File is too large. Maximum size is 10MB.');
        input.value = '';
        return;
    }

    // Validate file type
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) {
        alert('Unsupported image type. Use JPEG, PNG, or WebP.');
        input.value = '';
        return;
    }

    // Read file and show cropper
    const reader = new FileReader();
    reader.onload = function(e) {
        const image = document.getElementById('cropperImage');
        const container = document.getElementById('cropperContainer');
        const saveBtn = document.getElementById('saveUploadBtn');
        
        if (!image || !container || !saveBtn) {
            console.error('Required elements not found');
            return;
        }

        // Set image source
        image.src = e.target.result;
        
        // Show cropper container and save button
        container.style.display = 'block';
        saveBtn.style.display = 'inline-block';

        // Destroy existing cropper if any
        if (cropper) {
            cropper.destroy();
        }

        // Initialize Cropper.js
        cropper = new Cropper(image, {
            aspectRatio: 1,
            viewMode: 2,
            autoCropArea: 1,
            responsive: true,
            guides: true,
            center: true,
            highlight: true,
            background: false,
            cropBoxResizable: true,
            cropBoxMovable: true,
            dragMode: 'move',
            ready: function() {
                console.log('Cropper initialized');
            }
        });
    };
    reader.readAsDataURL(file);
};

window.saveAndUploadImage = async function() {
    const uploadBtnText = document.getElementById('uploadBtnText');
    const saveBtn = document.getElementById('saveUploadBtn');
    
    if (!cropper) {
        alert('Please select an image first');
        return;
    }

    // Disable button and show loading
    if (saveBtn) saveBtn.disabled = true;
    if (uploadBtnText) uploadBtnText.textContent = 'Uploading...';
    
    const success = await uploadCroppedImageDirect();
    
    if (success) {
        // Reload page to show new image
        window.location.href = '/edit/images';
    } else {
        alert('Failed to upload image. Please try again.');
        if (saveBtn) saveBtn.disabled = false;
        if (uploadBtnText) uploadBtnText.textContent = 'Save & Upload';
    }
};

window.uploadCroppedImageDirect = async function() {
    return new Promise((resolve, reject) => {
        if (!cropper) {
            console.error('Cropper not initialized');
            resolve(false);
            return;
        }

        cropper.getCroppedCanvas({
            width: 1024,
            height: 1024,
            imageSmoothingEnabled: true,
            imageSmoothingQuality: 'high'
        }).toBlob(async (blob) => {
            if (!blob) {
                console.error('Failed to create blob');
                resolve(false);
                return;
            }

            try {
                const formData = new FormData();
                formData.append('file', blob, 'profile.png');

                const response = await fetch('/api/profile-images/upload', {
                    method: 'POST',
                    body: formData,
                    credentials: 'include'
                });

                if (!response.ok) {
                    const errorData = await response.json().catch(() => ({}));
                    console.error('Upload failed:', errorData);
                    resolve(false);
                    return;
                }

                console.log('Upload successful');
                resolve(true);
            } catch (error) {
                console.error('Upload error:', error);
                resolve(false);
            }
        }, 'image/jpeg', 0.92);
    });
};

// Reset upload modal
window.resetUploadModal = function() {
    // Clean up cropper
    if (cropper) {
        cropper.destroy();
        cropper = null;
    }
    
    // Reset file input
    const fileInput = document.getElementById('imageFile');
    if (fileInput) fileInput.value = '';
    
    // Hide cropper container and save button
    const container = document.getElementById('cropperContainer');
    if (container) container.style.display = 'none';
    
    const saveBtn = document.getElementById('saveUploadBtn');
    if (saveBtn) {
        saveBtn.style.display = 'none';
        saveBtn.disabled = false;
    }
    
    const uploadBtnText = document.getElementById('uploadBtnText');
    if (uploadBtnText) uploadBtnText.textContent = 'Save & Upload';
};

