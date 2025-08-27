# Use the official Apache Airflow image as base
FROM apache/airflow:3.0.4

# Switch to root user to install system dependencies if needed
USER root

# Install any system dependencies here if required
# RUN apt-get update && apt-get install -y \
#     build-essential \
#     && rm -rf /var/lib/apt/lists/*

# Switch back to airflow user
USER airflow

# Copy requirements file
COPY requirements.txt /tmp/requirements.txt

# Install Python dependencies
RUN pip install --no-cache-dir -r /tmp/requirements.txt

# Copy DAGs and other files
COPY dags/ /opt/airflow/dags/
COPY plugins/ /opt/airflow/plugins/
COPY config/ /opt/airflow/config/

# Set the working directory
WORKDIR /opt/airflow
