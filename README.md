# C# Speed Rush

## Description

Turn-based racing simulation game where players manage fuel, speed, and strategy to complete 5 laps.

## How to Play

1. Select a car (Economy, Sport, or Formula)
2. Click "Start Race"
3. Use action buttons each turn:
   - **Speed Up**: Faster progress, more fuel consumption
   - **Maintain Speed**: Steady progress, normal fuel use
   - **Pit Stop**: Refuel (takes one turn to enter, one to complete)
4. Complete 5 laps before running out of fuel to win!

## How to Run

1. Clone repository
2. Open CSharpSpeedRush.sln in Visual Studio
3. Build solution (Ctrl+Shift+B)
4. Run project (F5)

## Technologies Used

- .NET WPF Framework
- NUnit for unit testing
- C# with object-oriented design

## Architecture

- **Models**: Car, RaceManager, Enums
- **UI**: WPF with data binding
- **Testing**: Comprehensive unit tests

// ============= Architecture.md =============

# Architecture Design Document

## Overview

C# Speed Rush follows a clean separation of concerns with distinct layers for business logic, data models, and user interface.

## Class Design

### Car Class

- **Purpose**: Represents vehicle attributes and state
- **Key Features**: Fuel management, speed tracking, type-specific properties
- **Data Structures**: Properties for stats, methods for fuel operations

### RaceManager Class

- **Purpose**: Central game controller managing race state
- **Key Features**: Turn-based logic, progress tracking, win/lose conditions
- **Data Structures**:
  - `List<Car>` for available vehicles
  - `Queue<ActionType>` for action history
  - Enums for type safety

### UI Layer (MainWindow)

- **Purpose**: User interface and event handling
- **Pattern**: Code-behind with event-driven updates
- **Features**: Real-time progress bars, responsive controls

## Technical Decisions

### Data Structures Justification

- **List<Car>**: Efficient random access for car selection
- **Queue<ActionType>**: FIFO structure perfect for action history
- **Enums**: Type safety and clear state representation

### Exception Handling Strategy

- Validation at method entry points
- Custom exceptions for domain-specific errors
- Graceful UI error display with MessageBox

## UML Class Diagram

```
[Car] --> [RaceManager]
[RaceManager] --> [MainWindow]
[Enums] <-- [Car, RaceManager]
```

## Testing Approach

- Unit tests cover core business logic
- Edge cases for fuel consumption and lap progression
- Exception testing for invalid operations
