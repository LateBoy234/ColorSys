# TCP Communication Unit Tests

## Overview
This directory contains comprehensive unit tests for the TCP communication implementation in the ColorSystem project.

## Test Coverage

### TcpCommunicationTests.cs
Provides complete coverage of the `TcpCommunication` class with the following test categories:

#### 1. Connection Tests
- ✅ Successful TCP connection to valid server
- ✅ Connection failure with invalid server
- ✅ Connection state after disposal

#### 2. Reconnection Tests
- ✅ Successful reconnection when server becomes available
- ✅ Reconnection failure when server is unavailable
- ✅ Multiple reconnection attempts

#### 3. State Change Event Tests
- ✅ StateChanged event fires during reconnection
- ✅ Correct state transitions (Reconnecting → Connected)

#### 4. SendAsync Tests
- ✅ Sending data successfully with connected client
- ✅ Exception handling when sending without connection
- ✅ Concurrent send operations

#### 5. Property Tests
- ✅ SupportsPlugDetect returns false for TCP
- ✅ IsConnected property behavior

#### 6. Integration Tests
- ✅ Complete connection lifecycle (connect → send → disconnect)
- ✅ Thread-safe concurrent operations

## Running Tests

### Visual Studio
1. Open Test Explorer (Test → Test Explorer)
2. Click "Run All" or right-click specific tests

### Command Line
```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~TcpCommunicationTests"

# Run specific test method
dotnet test --filter "Name=ConnectAsync_WithValidServer_ShouldConnectSuccessfully"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Visual Studio Code
1. Install C# Dev Kit extension
2. Use Testing sidebar or run from command palette

## Test Architecture

### Mock Server
Tests use a local TCP server running on `127.0.0.1:15000`:
- Automatically started/stopped per test
- Simulates real TCP server behavior
- Handles client connections asynchronously

### Test Patterns Used
- **AAA Pattern**: Arrange → Act → Assert
- **Fluent Assertions**: Readable assertion syntax
- **Proper Cleanup**: All resources disposed in TestCleanup

## Dependencies
- MSTest: Test framework
- FluentAssertions: Better assertion readability
- Moq: Mocking framework (for future use)

## Best Practices Demonstrated
1. ✅ Each test is independent and isolated
2. ✅ Proper resource cleanup in [TestCleanup]
3. ✅ Descriptive test names following convention
4. ✅ Test categories organized by functionality
5. ✅ Async/await properly used throughout
6. ✅ Exception testing with specific types

## Future Enhancements
- [ ] Add tests for heartbeat mechanism
- [ ] Add tests for data framing/parsing
- [ ] Add performance benchmarks
- [ ] Add timeout scenario tests
- [ ] Add network interruption simulation

## Troubleshooting

### Port Already in Use
If tests fail with "port already in use":
```bash
# Find process using port 15000
netstat -ano | findstr :15000

# Kill process (replace PID)
taskkill /F /PID <PID>
```

### Firewall Issues
Ensure Windows Firewall allows loopback connections on port 15000.

## Contributing
When adding new tests:
1. Follow existing naming conventions
2. Add proper [Description] attributes
3. Update this README with new coverage
4. Ensure cleanup in [TestCleanup]
