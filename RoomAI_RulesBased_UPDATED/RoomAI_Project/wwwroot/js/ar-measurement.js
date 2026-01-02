class ARMeasurement {
    constructor() {
        this.isARSupported = this.checkARSupport();
        this.measurements = [];
        this.startPosition = null;
        this.isActive = false;
    }

    checkARSupport() {
        const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent);
        const isAndroid = /Android/.test(navigator.userAgent);
        
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            return false;
        }

        return isIOS || isAndroid;
    }

    async startMeasurement() {
        if (!this.isARSupported) {
            alert('AR is not supported on this device');
            return false;
        }

        try {
            const stream = await navigator.mediaDevices.getUserMedia({
                video: { facingMode: 'environment' }
            });

            this.isActive = true;
            this.measurements = [];
            this.setupDeviceMotionListener();
            return true;
        } catch (error) {
            console.error('Camera access denied:', error);
            return false;
        }
    }

    setupDeviceMotionListener() {
        if (!window.DeviceMotionEvent) {
            console.warn('Device Motion API not supported');
            return;
        }

        window.addEventListener('devicemotion', (event) => {
            if (!this.isActive) return;

            const acceleration = event.accelerationIncludingGravity;
            this.measurements.push({
                x: acceleration.x || 0,
                y: acceleration.y || 0,
                z: acceleration.z || 0,
                timestamp: Date.now()
            });

            if (this.measurements.length > 1000) {
                this.measurements.shift();
            }
        });
    }

    calculateDistance() {
        if (this.measurements.length < 10) {
            return 0;
        }

        const recentMeasurements = this.measurements.slice(-50);
        let totalDistance = 0;

        for (let i = 1; i < recentMeasurements.length; i++) {
            const prev = recentMeasurements[i - 1];
            const curr = recentMeasurements[i];

            const dx = curr.x - prev.x;
            const dy = curr.y - prev.y;
            const dz = curr.z - prev.z;

            const distance = Math.sqrt(dx * dx + dy * dy + dz * dz);
            totalDistance += distance;
        }

        return totalDistance * 0.5;
    }

    stopMeasurement() {
        this.isActive = false;
        return this.calculateDistance();
    }
}

const arMeasurement = new ARMeasurement();

function measureWithAR() {
    const roomAreaInput = document.getElementById('roomArea');
    
    if (!arMeasurement.isARSupported) {
        alert('AR measurement is not available on this device. Please enter the area manually.');
        return;
    }

    const startBtn = document.createElement('button');
    startBtn.textContent = 'Start Measuring';
    startBtn.className = 'btn btn-primary';
    startBtn.onclick = async () => {
        const success = await arMeasurement.startMeasurement();
        if (success) {
            startBtn.textContent = 'Stop Measuring';
            startBtn.onclick = () => {
                const distance = arMeasurement.stopMeasurement();
                const estimatedArea = Math.max(10, distance * 0.8);
                roomAreaInput.value = estimatedArea.toFixed(1);
                startBtn.remove();
                alert(`Estimated area: ${estimatedArea.toFixed(1)} m²`);
            };
        }
    };

    roomAreaInput.parentElement.appendChild(startBtn);
}
