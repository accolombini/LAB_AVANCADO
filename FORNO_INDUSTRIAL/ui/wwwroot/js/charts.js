// Advanced SCADA Charts - Modernized Industrial Dashboard
window.FornoCharts = {
    temperatureChart: null,
    trendChart: null,
    statusPieChart: null,
    gaugeChart: null,

    // Configurações de cores do tema
    colors: {
        primary: '#00d4ff',
        success: '#00ff88',
        warning: '#ff9500',
        danger: '#ff3366',
        background: '#1e2a3e',
        text: '#ffffff',
        grid: '#2a3f5f'
    },

    // Inicializar Gauge de Temperatura 3D
    initTemperatureGauge: function(elementId, temperature, setpoint) {
        const data = [{
            domain: { x: [0, 1], y: [0, 1] },
            value: temperature,
            title: { 
                text: `<span style="color: #00d4ff; font-family: Orbitron; font-size: 18px;">Temperatura Atual</span><br><span style="color: #b0bec5; font-size: 14px;">Setpoint: ${setpoint}°C</span>`,
                font: { color: '#ffffff', size: 16 }
            },
            type: "indicator",
            mode: "gauge+number+delta",
            delta: { 
                reference: setpoint,
                valueformat: ".1f",
                suffix: "°C",
                font: { color: '#ffffff', size: 14 }
            },
            number: {
                valueformat: ".1f",
                suffix: "°C",
                font: { color: '#ffffff', size: 32, family: 'Orbitron' }
            },
            gauge: {
                axis: { 
                    range: [1000, 1900],
                    tickwidth: 2,
                    tickcolor: "#ffffff",
                    tickfont: { color: '#ffffff', size: 12 }
                },
                bar: { 
                    color: this.getGaugeColor(temperature),
                    thickness: 0.7
                },
                bgcolor: "rgba(30, 42, 62, 0.8)",
                borderwidth: 2,
                bordercolor: "#2a3f5f",
                steps: [
                    { range: [1000, 1400], color: "rgba(0, 212, 255, 0.2)" },
                    { range: [1400, 1600], color: "rgba(0, 255, 136, 0.2)" },
                    { range: [1600, 1750], color: "rgba(255, 149, 0, 0.2)" },
                    { range: [1750, 1900], color: "rgba(255, 51, 102, 0.2)" }
                ],
                threshold: {
                    line: { color: "#ff3366", width: 4 },
                    thickness: 0.75,
                    value: 1750
                }
            }
        }];

        const layout = {
            paper_bgcolor: 'transparent',
            plot_bgcolor: 'transparent',
            font: { color: '#ffffff' },
            margin: { l: 20, r: 20, t: 60, b: 20 },
            annotations: [{
                text: 'FORNO INDUSTRIAL',
                x: 0.5,
                y: -0.015,
                xref: 'paper',
                yref: 'paper',
                showarrow: false,
                font: { 
                    color: '#607d8b', 
                    size: 11
                }
            }]
        };

        const config = {
            responsive: true,
            displayModeBar: false
        };

        Plotly.newPlot(elementId, data, layout, config);
        this.gaugeChart = document.getElementById(elementId);
    },

    // Atualizar Gauge
    updateTemperatureGauge: function(elementId, temperature, setpoint) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.warn(`⚠️ Elemento ${elementId} não encontrado, inicializando gauge...`);
            this.initTemperatureGauge(elementId, temperature, setpoint);
            return;
        }

        // Verificar se o Plotly foi inicializado no elemento
        if (!element.data || !Array.isArray(element.data) || element.data.length === 0) {
            console.warn(`⚠️ Dados do Plotly não encontrados em ${elementId}, reinicializando...`);
            this.initTemperatureGauge(elementId, temperature, setpoint);
            return;
        }

        try {
            // Verificar se o element tem dados válidos para Plotly
            if (!element.data || !Array.isArray(element.data) || element.data.length === 0 || !element.data[0]) {
                console.warn(`⚠️ Dados do elemento inválidos, reinicializando gauge...`);
                this.initTemperatureGauge(elementId, temperature, setpoint);
                return;
            }

            Plotly.restyle(element, {
                'value': [temperature],
                'delta.reference': [setpoint],
                'gauge.bar.color': [this.getGaugeColor(temperature)],
                'title.text': [`<span style="color: #00d4ff; font-family: Orbitron; font-size: 18px;">Temperatura Atual</span><br><span style="color: #b0bec5; font-size: 14px;">Setpoint: ${setpoint}°C</span>`]
            }, [0]);
        } catch (error) {
            console.error('❌ Erro ao atualizar gauge:', error);
            // Se falhar, reinicializar o gauge
            this.initTemperatureGauge(elementId, temperature, setpoint);
        }
    },

    // Obter cor do gauge baseada na temperatura
    getGaugeColor: function(temperature) {
        if (temperature >= 1750) return "#ff3366"; // Crítico
        if (temperature >= 1600) return "#ff9500"; // Alarme
        if (temperature >= 1400) return "#00ff88"; // Normal operação
        return "#00d4ff"; // Aquecimento
    },

    // Inicializar gráfico de tendência avançado
    initTrendChart: function(elementId) {
        const data = [
            {
                x: [],
                y: [],
                type: 'scatter',
                mode: 'lines+markers',
                name: 'Temperatura Atual',
                line: { 
                    color: '#00ff88', 
                    width: 3,
                    shape: 'spline'
                },
                marker: { 
                    size: 6,
                    color: '#00ff88',
                    line: { color: '#ffffff', width: 1 }
                },
                fill: 'tozeroy',
                fillcolor: 'rgba(0, 255, 136, 0.1)'
            },
            {
                x: [],
                y: [],
                type: 'scatter',
                mode: 'lines',
                name: 'Setpoint',
                line: { 
                    color: '#00d4ff', 
                    width: 2, 
                    dash: 'dash' 
                }
            },
            {
                x: [],
                y: [],
                type: 'scatter',
                mode: 'lines',
                name: 'Limite Alarme',
                line: { 
                    color: '#ff9500', 
                    width: 2,
                    dash: 'dot'
                }
            },
            {
                x: [],
                y: [],
                type: 'scatter',
                mode: 'lines',
                name: 'Limite Crítico',
                line: { 
                    color: '#ff3366', 
                    width: 2,
                    dash: 'dot'
                }
            }
        ];

        const layout = {
            title: {
                text: '<span style="color: #00d4ff; font-family: Orbitron;">Monitoramento Contínuo de Temperatura</span>',
                font: { size: 18 }
            },
            paper_bgcolor: 'transparent',
            plot_bgcolor: 'rgba(30, 42, 62, 0.3)',
            font: { color: '#ffffff', family: 'Roboto' },
            xaxis: {
                title: { 
                    text: 'Tempo',
                    font: { color: '#b0bec5', size: 14 }
                },
                gridcolor: '#2a3f5f',
                tickformat: '%H:%M:%S',
                showgrid: true,
                zeroline: false,
                tickfont: { color: '#b0bec5' }
            },
            yaxis: {
                title: { 
                    text: 'Temperatura (°C)',
                    font: { color: '#b0bec5', size: 14 }
                },
                gridcolor: '#2a3f5f',
                showgrid: true,
                zeroline: false,
                range: [1000, 1900],
                tickfont: { color: '#b0bec5' }
            },
            legend: {
                x: 0.02,
                y: 0.98,
                bgcolor: 'rgba(30, 42, 62, 0.8)',
                bordercolor: '#2a3f5f',
                borderwidth: 1,
                font: { color: '#ffffff', size: 12 }
            },
            margin: { l: 60, r: 30, t: 60, b: 60 },
            hovermode: 'x unified',
            hoverlabel: {
                bgcolor: 'rgba(30, 42, 62, 0.9)',
                bordercolor: '#00d4ff',
                font: { color: '#ffffff' }
            }
        };

        const config = {
            responsive: true,
            displayModeBar: true,
            displaylogo: false,
            modeBarButtonsToRemove: ['pan2d', 'select2d', 'lasso2d', 'autoScale2d', 'resetScale2d']
        };

        Plotly.newPlot(elementId, data, layout, config);
        this.trendChart = document.getElementById(elementId);
    },

    // Atualizar gráfico de tendência
    updateTrendChart: function(elementId, timestamp, temperature, setpoint, alarmTemp, criticalTemp) {
        const element = document.getElementById(elementId);
        if (!element) {
            this.initTrendChart(elementId);
            return; // Aguarda próxima chamada após inicialização
        }

        // Verifica se o gráfico tem dados válidos
        if (!element.data || !Array.isArray(element.data) || element.data.length === 0) {
            console.log('⚠️ Trend chart não inicializado adequadamente, pulando atualização');
            return;
        }

        try {
            const time = new Date(timestamp);
            
            Plotly.extendTraces(elementId, {
                x: [[time], [time], [time], [time]],
                y: [[temperature], [setpoint], [alarmTemp], [criticalTemp]]
            }, [0, 1, 2, 3]);
        } catch (error) {
            console.error('❌ Erro ao atualizar trend chart:', error);
            return;
        }

        // Manter apenas últimos 100 pontos
        const currentData = document.getElementById(elementId).data;
        if (currentData[0].x.length > 100) {
            const start = currentData[0].x.length - 100;
            const end = currentData[0].x.length - 1;
            Plotly.relayout(elementId, {
                'xaxis.range': [currentData[0].x[start], currentData[0].x[end]]
            });
        }
    },

    // Inicializar gráfico circular de status
    initStatusPieChart: function(elementId) {
        const data = [{
            values: [40, 30, 20, 10],
            labels: ['Aquecendo', 'Mantendo', 'Resfriando', 'Alarme'],
            type: 'pie',
            hole: 0.4,
            marker: {
                colors: ['#00ff88', '#00ff88', '#00d4ff', '#ff9500'],
                line: {
                    color: '#ffffff',
                    width: 2
                }
            },
            textfont: {
                color: '#ffffff',
                size: 14,
                family: 'Roboto'
            },
            hovertemplate: '<b>%{label}</b><br>%{percent}<br><extra></extra>',
            hoverlabel: {
                bgcolor: 'rgba(30, 42, 62, 0.9)',
                bordercolor: '#00d4ff',
                font: { color: '#ffffff' }
            }
        }];

        const layout = {
            title: {
                text: '<span style="color: #00d4ff; font-family: Orbitron;">Distribuição Operacional</span>',
                font: { size: 16 }
            },
            paper_bgcolor: 'transparent',
            plot_bgcolor: 'transparent',
            font: { color: '#ffffff' },
            margin: { l: 20, r: 20, t: 60, b: 20 },
            showlegend: true,
            legend: {
                orientation: "v",
                x: 1.02,
                y: 0.5,
                font: { color: '#ffffff', size: 12 }
            },
            annotations: [{
                text: '<span style="color: #b0bec5; font-family: Orbitron; font-size: 14px;">STATUS<br>ATUAL</span>',
                x: 0.5,
                y: 0.5,
                showarrow: false,
                font: { size: 14 }
            }]
        };

        const config = {
            responsive: true,
            displayModeBar: false
        };

        Plotly.newPlot(elementId, data, layout, config);
        this.statusPieChart = document.getElementById(elementId);
    },

    // Atualizar gráfico circular
    updateStatusPieChart: function(elementId, stateDistribution) {
        if (!document.getElementById(elementId) || !stateDistribution) {
            return;
        }

        const states = Object.keys(stateDistribution);
        const values = Object.values(stateDistribution);
        const colors = states.map(state => this.getStateColor(state));

        const element = document.getElementById(elementId);
        if (!element || !element.data || !element.data.length) {
            console.warn(`⚠️ Elemento ${elementId} não encontrado ou sem dados para pie chart`);
            return;
        }

        Plotly.restyle(element, {
            values: [values],
            labels: [states],
            'marker.colors': [colors]
        });
    },

    // Obter cor do estado
    getStateColor: function(state) {
        const stateLower = state.toLowerCase();
        if (stateLower.includes('critica') || stateLower.includes('emergency')) return '#ff3366';
        if (stateLower.includes('alarme') || stateLower.includes('warning')) return '#ff9500';
        if (stateLower.includes('aquecendo') || stateLower.includes('heating')) return '#00ff88';
        if (stateLower.includes('resfriando') || stateLower.includes('cooling')) return '#00d4ff';
        if (stateLower.includes('mantendo') || stateLower.includes('normal')) return '#00ff88';
        return '#607d8b';
    },

    // Limpar todos os gráficos
    clearAllCharts: function() {
        if (this.gaugeChart) {
            Plotly.purge(this.gaugeChart);
            this.gaugeChart = null;
        }
        if (this.trendChart) {
            Plotly.purge(this.trendChart);
            this.trendChart = null;
        }
        if (this.statusPieChart) {
            Plotly.purge(this.statusPieChart);
            this.statusPieChart = null;
        }
    },

    // Redimensionar gráficos
    resizeCharts: function() {
        if (this.gaugeChart) Plotly.Plots.resize(this.gaugeChart);
        if (this.trendChart) Plotly.Plots.resize(this.trendChart);
        if (this.statusPieChart) Plotly.Plots.resize(this.statusPieChart);
    }
};

// Efeitos visuais adicionais
window.ScadaEffects = {
    // Animar conexão
    animateConnection: function(isConnected) {
        const indicators = document.querySelectorAll('.status-indicator');
        indicators.forEach(indicator => {
            if (isConnected) {
                indicator.classList.add('status-online');
                indicator.classList.remove('status-offline');
            } else {
                indicator.classList.add('status-offline');
                indicator.classList.remove('status-online');
            }
        });
    },

    // Animar valor de temperatura
    animateTemperatureValue: function(elementId, newValue) {
        const element = document.getElementById(elementId);
        if (element) {
            element.style.transform = 'scale(1.1)';
            setTimeout(() => {
                element.style.transform = 'scale(1)';
            }, 200);
        }
    },

    // Animar alarme crítico
    triggerCriticalAlarm: function() {
        document.body.classList.add('critical-alarm-active');
        setTimeout(() => {
            document.body.classList.remove('critical-alarm-active');
        }, 5000);
    }
};

// Funções globais para serem chamadas do Blazor
window.initTemperatureGauge = function(elementId, temperature, setpoint) {
    if (typeof Plotly === 'undefined') {
        console.error('❌ Plotly não está carregado');
        return;
    }
    console.log('✅ Inicializando gauge de temperatura');
    window.FornoCharts.initTemperatureGauge(elementId, temperature, setpoint);
};

window.updateTemperatureGauge = function(elementId, temperature, setpoint) {
    if (typeof Plotly === 'undefined') {
        console.error('❌ Plotly não está carregado para atualização do gauge');
        return;
    }
    
    if (!window.FornoCharts) {
        console.error('❌ FornoCharts não está inicializado');
        return;
    }
    
    try {
        console.log(`🔄 Atualizando gauge: ${elementId}, Temp: ${temperature}°C, Setpoint: ${setpoint}°C`);
        window.FornoCharts.updateTemperatureGauge(elementId, temperature, setpoint);
    } catch (error) {
        console.error('❌ Erro ao atualizar gráficos:', error);
        // Tentar reinicializar em caso de erro
        console.log('🔄 Tentando reinicializar gauge...');
        window.FornoCharts.initTemperatureGauge(elementId, temperature, setpoint);
    }
};

window.initTrendChart = function(elementId) {
    if (typeof Plotly === 'undefined') {
        console.error('❌ Plotly não está carregado');
        return;
    }
    console.log('✅ Inicializando gráfico de tendência');
    window.FornoCharts.initTrendChart(elementId);
};

window.updateTrendChart = function(elementId, timestamp, temperature, setpoint, alarmTemp, criticalTemp) {
    if (typeof Plotly === 'undefined') {
        console.error('❌ Plotly não está carregado para atualização do gráfico');
        return;
    }
    window.FornoCharts.updateTrendChart(elementId, timestamp, temperature, setpoint, alarmTemp, criticalTemp);
};

window.initStatusPieChart = function(elementId) {
    if (typeof Plotly === 'undefined') {
        console.error('❌ Plotly não está carregado');
        return;
    }
    console.log('✅ Inicializando gráfico circular');
    window.FornoCharts.initStatusPieChart(elementId);
};

window.updateStatusPieChart = function(elementId, stateDistribution) {
    if (typeof Plotly === 'undefined') {
        console.error('❌ Plotly não está carregado para atualização do gráfico circular');
        return;
    }
    window.FornoCharts.updateStatusPieChart(elementId, stateDistribution);
};

// Auto-inicialização e responsividade
document.addEventListener('DOMContentLoaded', function() {
    console.log('📊 Charts.js carregado');
    
    // Verificar se Plotly está disponível
    function checkPlotlyLoaded() {
        if (typeof Plotly !== 'undefined') {
            console.log('✅ Plotly detectado e pronto');
            
            // Redimensionar gráficos quando a janela muda de tamanho
            window.addEventListener('resize', function() {
                setTimeout(() => {
                    if (window.FornoCharts) {
                        window.FornoCharts.resizeCharts();
                    }
                }, 100);
            });
        } else {
            console.log('⏳ Aguardando Plotly carregar...');
            setTimeout(checkPlotlyLoaded, 500);
        }
    }
    
    checkPlotlyLoaded();
});

// Limpar recursos quando a página é descarregada
window.addEventListener('beforeunload', function() {
    window.FornoCharts.clearAllCharts();
});

// Função para atualizar timestamp em tempo real
setInterval(function() {
    const timestampElements = document.querySelectorAll('.timestamp');
    timestampElements.forEach(element => {
        element.textContent = new Date().toLocaleTimeString('pt-BR');
    });
}, 1000);
