import React, { useState } from 'react';
import { Button, List, Modal, Form, Input, DatePicker, Select, Row, Col, message, Popconfirm, Typography, Tag, Space } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, ProjectOutlined, LinkOutlined, GithubOutlined } from '@ant-design/icons';
import { useProjects } from '../../../hooks/useProfile';
import type { Project } from '../../../types/profile';
import dayjs, { type Dayjs } from 'dayjs';

const { Title, Text } = Typography;
const { TextArea } = Input;
const { Option } = Select;

const ProjectsTab: React.FC = () => {
  const { projects, loading, addProject, updateProject, deleteProject } = useProjects();
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [editingProject, setEditingProject] = useState<Project | null>(null);
  const [form] = Form.useForm();
  const [submitting, setSubmitting] = useState(false);



  const showModal = (project?: Project) => {
    setEditingProject(project || null);
    setIsModalVisible(true);
    
    if (project) {
      form.setFieldsValue({
        ...project,
        startDate: dayjs(project.startDate),
        endDate: project.endDate ? dayjs(project.endDate) : undefined,
        technologies: Array.isArray(project.technologies) 
          ? project.technologies.join(', ')
          : typeof project.technologies === 'string' 
            ? project.technologies 
            : ''
      });
    } else {
      form.resetFields();
    }
  };

  const handleCancel = () => {
    setIsModalVisible(false);
    setEditingProject(null);
    form.resetFields();
  };

  const handleSubmit = async (values: {
    name: string;
    description: string;
    technologies: string;
    startDate: Dayjs;
    endDate?: Dayjs;
    link?: string;
    repositoryUrl?: string;
    status: 'In Progress' | 'Completed' | 'On Hold';
  }) => {
    try {
      setSubmitting(true);
      const projectData = {
        name: values.name,
        description: values.description,
        technologies: values.technologies.split(',').map(tech => tech.trim()).filter(tech => tech).join(','),
        startDate: values.startDate.format('YYYY-MM'),
        endDate: values.endDate ? values.endDate.format('YYYY-MM') : undefined,
        projectUrl: values.link,
        githubUrl: values.repositoryUrl
      };

      if (editingProject) {
        await updateProject(editingProject.id, projectData);
        message.success('Project updated successfully!');
      } else {
        await addProject(projectData);
        message.success('Project added successfully!');
      }

      handleCancel();
    } catch {
      message.error(`Failed to ${editingProject ? 'update' : 'add'} project`);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteProject(id);
      message.success('Project deleted successfully!');
    } catch {
      message.error('Failed to delete project');
    }
  };

  const formatDate = (date: string) => {
    return dayjs(date).format('MMM YYYY');
  };

  const getPeriod = (startDate: string, endDate?: string) => {
    const start = formatDate(startDate);
    const end = endDate ? formatDate(endDate) : 'Present';
    return `${start} - ${end}`;
  };

  return (
    <div className="projects-tab">
      <div className="projects-tab__header">
        <Title level={3}>Projects</Title>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => showModal()}
        >
          Add Project
        </Button>
      </div>

      <List
        loading={loading}
        dataSource={projects}
        renderItem={(project) => (
          <List.Item
            actions={[
              <Button
                key="edit"
                type="text"
                icon={<EditOutlined />}
                onClick={() => showModal(project)}
              />,
              <Popconfirm
                key="delete"
                title="Are you sure you want to delete this project?"
                onConfirm={() => handleDelete(project.id)}
                okText="Yes"
                cancelText="No"
              >
                <Button
                  type="text"
                  danger
                  icon={<DeleteOutlined />}
                />
              </Popconfirm>
            ]}
          >
            <List.Item.Meta
              avatar={<ProjectOutlined style={{ fontSize: '24px', color: 'var(--color-primary)' }} />}
              title={
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px', flexWrap: 'wrap' }}>
                  <Text strong>{project.name}</Text>
                  {(project.projectUrl || project.githubUrl) && (
                    <Space size="small">
                      {project.projectUrl && (
                        <Button
                          type="link"
                          size="small"
                          icon={<LinkOutlined />}
                          href={project.projectUrl}
                          target="_blank"
                          style={{ padding: 0, height: 'auto' }}
                        >
                          Demo
                        </Button>
                      )}
                      {project.githubUrl && (
                        <Button
                          type="link"
                          size="small"
                          icon={<GithubOutlined />}
                          href={project.githubUrl}
                          target="_blank"
                          style={{ padding: 0, height: 'auto' }}
                        >
                          Code
                        </Button>
                      )}
                    </Space>
                  )}
                </div>
              }
              description={
                <div>
                  <Text>{project.description}</Text>
                  <br />
                  <Text type="secondary">{getPeriod(project.startDate, project.endDate)}</Text>
                  <br />
                  <div style={{ marginTop: '8px' }}>
                    {project.technologies && typeof project.technologies === 'string' && 
                      project.technologies.split(',').map((tech, index) => (
                        <Tag key={index} style={{ marginBottom: '4px' }}>
                          {tech.trim()}
                        </Tag>
                      ))
                    }
                    {project.technologies && Array.isArray(project.technologies) && 
                      project.technologies.map((tech, index) => (
                        <Tag key={index} style={{ marginBottom: '4px' }}>
                          {tech}
                        </Tag>
                      ))
                    }
                  </div>
                </div>
              }
            />
          </List.Item>
        )}
        locale={{ emptyText: 'No projects added yet' }}
      />

      <Modal
        title={editingProject ? 'Edit Project' : 'Add Project'}
        open={isModalVisible}
        onCancel={handleCancel}
        footer={null}
        width={700}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <Form.Item
            label="Project Name"
            name="name"
            rules={[{ required: true, message: 'Please enter the project name' }]}
          >
            <Input placeholder="e.g., Pancakes Blog Platform" />
          </Form.Item>

          <Form.Item
            label="Description"
            name="description"
            rules={[{ required: true, message: 'Please enter project description' }]}
          >
            <TextArea
              rows={3}
              placeholder="Describe what this project does..."
              maxLength={300}
              showCount
            />
          </Form.Item>

          <Form.Item
            label="Technologies"
            name="technologies"
            rules={[{ required: true, message: 'Please enter technologies used' }]}
            extra="Separate technologies with commas (e.g., React, TypeScript, Node.js)"
          >
            <Input placeholder="React, TypeScript, Node.js, PostgreSQL" />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label="Start Date"
                name="startDate"
                rules={[{ required: true, message: 'Please select start date' }]}
              >
                <DatePicker
                  picker="month"
                  placeholder="Select start date"
                  style={{ width: '100%' }}
                  format="MM/YYYY"
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                label="End Date"
                name="endDate"
              >
                <DatePicker
                  picker="month"
                  placeholder="Select end date (leave empty if ongoing)"
                  style={{ width: '100%' }}
                  format="MM/YYYY"
                />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            label="Status"
            name="status"
            rules={[{ required: true, message: 'Please select project status' }]}
          >
            <Select placeholder="Select project status">
              <Option value="In Progress">In Progress</Option>
              <Option value="Completed">Completed</Option>
              <Option value="On Hold">On Hold</Option>
            </Select>
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label="Demo Link"
                name="link"
              >
                <Input placeholder="https://your-project-demo.com" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                label="Repository URL"
                name="repositoryUrl"
              >
                <Input placeholder="https://github.com/user/project" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Button onClick={handleCancel} style={{ marginRight: 8 }}>
              Cancel
            </Button>
            <Button type="primary" htmlType="submit" loading={submitting}>
              {editingProject ? 'Update' : 'Add'} Project
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default ProjectsTab;
