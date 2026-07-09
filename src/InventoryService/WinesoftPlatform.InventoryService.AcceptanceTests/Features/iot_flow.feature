Feature: IoT Telemetry and Stock Alerts Flow
  As an operator of WineSoft
  I want to receive sensor telemetry and stock updates
  So that I can be alerted in case of critical anomalies or stock shortages

  Scenario: Ingesting normal sensor telemetry
    Given the IoT simulator generates a normal temperature reading of 20.0 °C
    When the telemetry reading is sent to the sensor alerts endpoint
    Then the reading should be persisted in the system
    And the status should be "NORMAL"
    And the reading should not be marked as an anomaly

  Scenario: Generating a critical temperature alert
    Given the IoT simulator generates a critical temperature reading of 32.5 °C
    When the telemetry reading is sent to the sensor alerts endpoint
    Then a critical alert should be created in the system
    And the status should be "CRITICAL"
    And the reading should be marked as an anomaly

  Scenario: Generating a critical stock alert
    Given the stock of a supply falls to 5 units
    When the supply quantity is updated in the inventory
    Then a critical stock alert should be generated for the supply
    And the status of the alert should be "CRITICAL"
    And the alert should be marked as an anomaly

  Scenario: Listing active alerts
    Given there are several active sensor alerts in the database
    When the operator requests the list of all active alerts
    Then the system should return a paginated list of alerts
    And the alerts should be ordered by timestamp descending
