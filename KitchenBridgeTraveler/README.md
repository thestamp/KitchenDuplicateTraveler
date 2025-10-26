# Kitchen-Duplicate-Traveler

A duplicate bridge traveler scoring application for calculating match points.

## Features

- Calculate match points for duplicate bridge games
- Handle tie scenarios with proper averaging
- Parse PBN (Portable Bridge Notation) files
- Generate formatted PDF reports with results

## Console Application Usage

The console application processes PBN files and generates PDF reports with match point calculations.

### How to Use

1. Build the solution in Release mode
2. Locate the `Traveler.Console.exe` executable in the output directory
3. Drag and drop a PBN file onto `Traveler.Console.exe`
4. The application will generate a PDF in the same folder as the input file with the suffix `_Results.pdf`

### PDF Output

- **Layout**: Landscape orientation, 2 boards per page (side-by-side)
- **Content**: 
  - Board number, dealer, and vulnerability
  - All four hands with card holdings
  - Match points table showing all possible scores and their rankings
- **Format**: Bordered sections with clear headers and formatted card holdings using suit symbols

### Input File Format

The application accepts **PBN (Portable Bridge Notation)** format files. This is the standard format for bridge games and includes:
- Board information (dealer, vulnerability)
- Card deals for all four players
- Score tables with contract results

See `test.pbn` in the test project for an example.

## Development

This project uses .NET 8 and includes:
- `Traveler.Core`: Core business logic for match point calculations and PBN parsing
- `Traveler.Core.Tests`: Unit tests
- `Traveler.Console`: Console application for file processing and PDF generation

### Dependencies

- QuestPDF: For PDF generation with beautiful layouts
