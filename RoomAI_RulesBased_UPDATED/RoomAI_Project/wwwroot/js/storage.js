class StorageManager {
    static setSurfaceMode(mode) {
        if (!['walls', 'floors', 'both'].includes(mode)) {
            throw new Error('Invalid surface mode');
        }
        localStorage.setItem('surfaceMode', mode);
        sessionStorage.setItem('surfaceMode', mode);
    }

    static getSurfaceMode() {
        return localStorage.getItem('surfaceMode') || 'both';
    }

    static setImageData(imageId, imageUrl) {
        localStorage.setItem('imageId', imageId);
        localStorage.setItem('imageUrl', imageUrl);
        sessionStorage.setItem('imageId', imageId);
    }

    static getImageData() {
        return {
            imageId: localStorage.getItem('imageId'),
            imageUrl: localStorage.getItem('imageUrl')
        };
    }

    static setResultData(resultImageUrl, originalImageUrl) {
        localStorage.setItem('resultImage', resultImageUrl);
        localStorage.setItem('originalImage', originalImageUrl);
        sessionStorage.setItem('resultImage', resultImageUrl);
    }

    static getResultData() {
        return {
            resultImage: localStorage.getItem('resultImage'),
            originalImage: localStorage.getItem('originalImage')
        };
    }

    static setRoomInfo(roomArea, roomType) {
        localStorage.setItem('roomArea', roomArea);
        localStorage.setItem('roomType', roomType);
        sessionStorage.setItem('roomArea', roomArea);
        sessionStorage.setItem('roomType', roomType);
    }

    static getRoomInfo() {
        return {
            roomArea: localStorage.getItem('roomArea'),
            roomType: localStorage.getItem('roomType')
        };
    }

    static setDesignChoices(palette, material) {
        localStorage.setItem('selectedPalette', JSON.stringify(palette));
        localStorage.setItem('selectedMaterial', JSON.stringify(material));
    }

    static getDesignChoices() {
        return {
            palette: JSON.parse(localStorage.getItem('selectedPalette') || 'null'),
            material: JSON.parse(localStorage.getItem('selectedMaterial') || 'null')
        };
    }

    static clearAll() {
        localStorage.clear();
        sessionStorage.clear();
    }

    static clearSession() {
        sessionStorage.clear();
    }
}
