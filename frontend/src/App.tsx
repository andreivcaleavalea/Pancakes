import React from 'react';
import { App as AntdApp, ConfigProvider } from 'antd';
import MainLayout from './layouts/MainLayout/MainLayout';
import HomePage from './pages/HomePage/HomePage';
import './styles/globals.scss';
import './App.css';

const App: React.FC = () => {
  return (
    <ConfigProvider>
      <AntdApp>
        <MainLayout>
          <HomePage />
        </MainLayout>
      </AntdApp>
    </ConfigProvider>
  );
};

export default App;