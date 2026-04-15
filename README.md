# Horror Prototype – Motores I

## Descripción

Prototipo de videojuego de terror desarrollado en Unity.  
El enfoque principal está en generar tensión mediante un enemigo reactivo basado en percepción (visión y sonido), evitando sistemas tradicionales como combate directo o HUD explícito.

El proyecto prioriza la **experiencia del jugador** y el **game feel** a través de iteración constante.

---

## Objetivo del Proyecto

Validar un sistema de IA que genere presión constante sobre el jugador mediante:

- Percepción visual (línea de visión)
- Percepción auditiva (eventos de sonido)
- Comportamiento dinámico basado en estados
- Reposicionamiento inteligente del enemigo (teleport)

---

## Core Loop

1. El jugador se mueve y genera ruido
2. El enemigo percibe (visión / sonido)
3. El enemigo responde (investiga / persigue)
4. El jugador evade / se oculta
5. El sistema ajusta tensión constantemente

---

## Controles

- Movimiento: WASD
- Cámara: Mouse
- Sprint / Stealth: (configurado en input system)
- Interacción: E

---

## Sistemas Implementados

### IA del Enemigo (FSM)

Estados principales:

- Idle → patrullaje sin estímulos
- Investigate → responde a sonidos
- Chase → persigue al jugador
- Retreat → comportamiento psicológico (zona segura)

---

### Sistema de Percepción

- Visión:
  - Distancia
  - Ángulo dinámico
  - Raycast (detección real)

- Audición:
  - Eventos de sonido con intensidad
  - Prioridad por tiempo (último evento relevante)

---

### Sistema de Ruido

- Generación de ruido por movimiento del jugador
- Eventos discretos para IA (no continuo)
- Separación conceptual:
  - UI Noise (feedback)
  - Sound Events (IA)

---

### Sistema de Movimiento

- Player controller basado en CharacterController
- Velocidades diferenciadas:
  - Walk
  - Sprint
  - Stealth

---

### Sistema de Audio Reactivo

- Ambient (constante)
- Whisper (proximidad sin amenaza)
- Attack (impacto en peligro)

Basado en:
- Distancia al jugador
- Estado del enemigo

---

### Sistema de Interacción

- Interactables genéricos
- Puertas con lógica de apertura contextual
- Interacción tanto por jugador como por enemigo

---

### Sistema de Zonas Seguras

- El jugador puede entrar en estado seguro
- El enemigo deja de perseguir
- Se limpian estímulos de sonido

---

### Sistema de Teleport del Enemigo (Repositioning)

- Solo activo en estado Idle
- Se activa si:
  - Está muy lejos del jugador
  - No recibe estímulos por tiempo prolongado

Características:

- Selección de punto en NavMesh válido
- Evita visibilidad directa del jugador
- Respeta distancia mínima y máxima
- No interrumpe estados activos (Investigate / Chase)

Objetivo:  
Mantener presión constante y evitar estados muertos de gameplay

---

## Estado del Proyecto

**In Development – Core Systems Functional**

- Gameplay base validado
- IA funcional
- Sistemas principales implementados

Pendiente:
- Pulido
- Mejora de feedback
- Refactor arquitectural (SRP)

---

## Bugs Conocidos

- Posibles inconsistencias en visibilidad del teleport
- Ajustes pendientes en balance de distancias
- Sistemas aún acoplados en ShadowEnemy (God Object temporal)

---

## Decisiones de Diseño

- No hay combate directo → foco en evasión
- IA centralizada para iteración rápida
- Uso de eventos de sonido en lugar de ruido continuo
- Teleport como herramienta de control de tensión (no mecánica visible)

---

## Tecnologías

- Unity
- C#
- NavMesh (AI Navigation)
- Input System (Unity)

---

## Arquitectura (Estado Actual)

- ShadowEnemy → controlador central (temporal)
- EnemyPerception → visión
- NoiseSystem → sonido
- PlayerNoiseEmitter → generación de eventos
- EnemyPresenceAudio → representación sonora

Pendiente:
Separación por SRP:
- EnemyBrain
- EnemyMovement
- EnemyHearing

---

## Próximos Pasos

1. Pulido del sistema de teleport (game feel)
2. Implementación de feedback visual (fade)
3. Mejora del sistema de percepción (precisión)
4. Refactor de arquitectura (separación de responsabilidades)
5. Ajuste de pacing y tensión
6. Integración completa de audio

---

## Contexto Académico

Proyecto desarrollado para:

Motores de Videojuegos I  
Tecnicatura en Desarrollo y Producción de Videojuegos

---

## Equipo

- Adriel Leonardo Agüero
- Vicentino Echeverria
- Valentin Herrera
- Juan Bautista Lebrero Joseph
- Alejandro M. Matesa
- Alan Ariel Palinzuela
