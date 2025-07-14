import React from 'react';
import { ConfigProvider } from 'antd';
import MainLayout from './layouts/MainLayout/MainLayout';
import HomePage from './pages/HomePage/HomePage';
import './styles/globals.scss';
import './App.css';

const theme = {
  token: {
    colorPrimary: '#E9833A', // Maple syrup orange
    colorSuccess: '#B16B2F', // Brown sugar
    colorWarning: '#FFB549', // Butter yellow
    colorInfo: '#F5DEB3', // Wheat/pancake color
    colorError: '#D14B1F', // Cinnamon
    fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
  },
};

const App: React.FC = () => {
  return (
    <ConfigProvider theme={theme}>
      <MainLayout>
        <HomePage />
      </MainLayout>
    </ConfigProvider>
  );
};

export default App;