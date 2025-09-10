// Forno Industrial SCADA - Chart Functions

window.ScadaChart = {
    currentChart: null,
    
    initTemperatureChart: function(elementId) {
        const layout = {
            title: {
                text: 'Tendência de Temperatura em Tempo Real',
                font: { color: '#ecf0f1', size: 16 }
            },
            xaxis: {
                title: 'Tempo',
                color: '#ecf0f1',
                gridcolor: '#34495e'
            },
            yaxis: {
                title: 'Temperatura (°C)',
                color: '#ecf0f1',
                gridcolor: '#34495e'
            },
            plot_bgcolor: '#2c3e50',
            paper_bgcolor: '#34495e',
            font: { color: '#ecf0f1' }
        };
        
        const data = [{
            x: [],
            y: [],
            type: 'scatter',
            mode: 'lines+markers',
            name: 'Temperatura',
            line: { color: '#3498db', width: 2 },
            marker: { color: '#3498db', size: 4 }
        }];
        
        Plotly.newPlot(elementId, data, layout, {responsive: true});
        this.currentChart = elementId;
    },
    
    updateTemperatureChart: function(elementId, timestamps, temperatures, states) {
        if (!timestamps || timestamps.length === 0) return;
        
        const colors = temperatures.map(temp => {
            if (temp >= 1800) return '#e74c3c'; // Crítico
            if (temp >= 1700) return '#f39c12'; // Alarme
            return '#27ae60'; // Normal
        });
        
        const update = {
            x: [timestamps],
            y: [temperatures],
            'marker.color': [colors]
        };
        
        Plotly.restyle(elementId, update, [0]);
    },
    
    addTemperaturePoint: function(elementId, timestamp, temperature, state) {
        if (!this.currentChart) return;
        
        const color = temperature >= 1800 ? '#e74c3c' : 
                     temperature >= 1700 ? '#f39c12' : '#27ae60';
        
        Plotly.extendTraces(elementId, {
            x: [[timestamp]],
            y: [[temperature]]
        }, [0]);
        
        // Manter apenas últimos 50 pontos
        Plotly.relayout(elementId, {
            'xaxis.range': [new Date(Date.now() - 300000), new Date()] // Últimos 5 minutos
        });
    }
};
